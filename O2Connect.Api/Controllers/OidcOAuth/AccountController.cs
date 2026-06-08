using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.DataFactories;
using O2Connect.Api.DataFactories.RequestModels;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.Collections.Immutable;
using System.Security.Claims;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("account")]
public class AccountController : ControllerBase
{
    private readonly ILoginService _loginService;
    private readonly IClientAuthenticationService _clientAuthenticationService;
    private readonly ITokenFactory _tokenFactory;

    public AccountController(
        ILoginService loginService,
        IClientAuthenticationService clientAuthenticationService,
        ITokenFactory tokenFactory)
    {
        _loginService = loginService;
        _clientAuthenticationService = clientAuthenticationService;
        _tokenFactory = tokenFactory;
    }

    [HttpPost("login")]
    public async Task<IActionResult> PostLogin([FromQuery(Name = "session")] string? sessionId,
                                               [FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Invalid credentials" });

        if (string.IsNullOrWhiteSpace(request.ClientId))
            return BadRequest(new { message = "Invalid client" });

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var response = await _loginService.HandleWithSessionAsync(request.Username.Trim(),
                                                                      request.Password,
                                                                      request.ClientId,
                                                                      sessionId,
                                                                      HttpContext.RequestAborted);
            return Ok(response);
        }

        var user = await _loginService.ValidateCredentialsAsync(request.Username.Trim(),
                                                                request.Password,
                                                                HttpContext.RequestAborted);

        var client = await _clientAuthenticationService.GetClientAsync(request.ClientId,
                                                                       HttpContext.RequestAborted);

        if (user is null || client is null)
            return Unauthorized(new { message = "Invalid credentials" });

        var allowedScopes = user.Scopes.Intersect(client.AllowedScopes).ToImmutableHashSet();

        if (allowedScopes.IsEmpty)
            return Forbid();

        var tokenFactoryRequest = new JwtTokenFactoryRequest
        {
            ClientId = client.ClientId,
            Subject = user.Id,
            Scopes = allowedScopes,
            AdditionalClaims = new Dictionary<string, object>
            {
                { "name", user.Username }
            }
        };

        if (user.Roles is not null && user.Roles.Count > 0)
        {
            tokenFactoryRequest.AdditionalClaims["roles"] = user.Roles.ToArray();
        }

        var tokenResponse = await _tokenFactory.GenerateAsync(tokenFactoryRequest,
                                                              HttpContext.RequestAborted);

        return Ok(tokenResponse);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { message = "Missing refresh token" });

        await _loginService.LogoutAsync(request.Token, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize(Policy = "RequireProfileScope")]
    public IActionResult Me()
    {
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";

        return Ok(new MyAccountResponse
        {
            IsAuthenticated = true,
            Id = User.FindFirstValue("sub") ?? string.Empty,
            Username = User.FindFirstValue("name") ?? string.Empty,
            Roles = User.FindAll("role").Select(r => r.Value).ToArray(),
        });
    }
}
