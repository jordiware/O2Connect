using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Security;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.Security.Claims;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("account")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(
        IAccountService loginService)
    {
        _accountService = loginService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> PostLogin([FromQuery(Name = "session")] string? sessionId,
                                               [FromBody] LoginRequest request,
                                               CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var result = await _accountService.HandleLoginAsync(sessionId, request, ct);

        return result switch
        {
            LoginRedirect r => Ok(r.RedirectResponse),
            LoginTokenSuccess r => Ok(r.TokenResponse),
            _ => throw OAuthException.FromServerError()
        };
    }

    [HttpPost("logout")]
    [RequireUserToken]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { message = "Missing refresh token" });

        await _accountService.HandleLogoutAsync(request.Token, ct);

        return NoContent();
    }

    [HttpGet("me")]
    [RequireUserToken]
    [RequireScope(Scopes.Users.Read)]
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

    [HttpPost("register")]
    [RequireClientToken]
    [RequireScope(Scopes.Account.Register)]
    public async Task<IActionResult> PostRegister([FromBody] RegisterUserRequest request,
                                                  CancellationToken ct)
    {
        var result = await _accountService.HandleRegisterAsync(request, ct);

        return Ok(result);
    }
}
