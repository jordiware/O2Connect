using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.DataFactories;
using O2Connect.Api.DataFactories.RequestModels;
using O2Connect.Api.Models.Store;
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
    private readonly ITokenFactory _tokenFactory;

    public AccountController(
        ILoginService loginService,
        ITokenFactory tokenFactory)
    {
        _loginService = loginService;
        _tokenFactory = tokenFactory;
    }

    [HttpPost("login")]
    public async Task<IActionResult> PostLogin([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Invalid credentials" });

        var user = await _loginService.ValidateCredentialsAsync(request.Username.Trim(),
                                                                request.Password,
                                                                HttpContext.RequestAborted);

        if (user is null)
            return Unauthorized(new { message = "Invalid credentials" });

        var tokenFactoryRequest = new JwtTokenFactoryRequest
        {
            ClientId = request.ClientId,
            Subject = user.Id,
            Scopes = user.Scopes.ToImmutableHashSet(),
            AdditionalClaims = new Dictionary<string, object>
            {
                { "name", user.Username }
            }
        };

        if (!string.IsNullOrWhiteSpace(user.Role))
        {
            tokenFactoryRequest.AdditionalClaims["role"] = user.Role;
        }

        var tokenResponse = await _tokenFactory.GenerateAsync(tokenFactoryRequest, HttpContext.RequestAborted);

        return Ok(tokenResponse);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok("/auth/logout endpoint");
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(MyAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
