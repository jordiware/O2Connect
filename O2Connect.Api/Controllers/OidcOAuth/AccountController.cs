using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.Security.Claims;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("account")]
public class AccountController : OidcOAuthControllerBase
{
    private readonly IAccountService _loginService;

    public AccountController(
        IAccountService loginService)
    {
        _loginService = loginService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> PostLogin([FromQuery(Name = "session")] string? sessionId,
                                               [FromBody] LoginRequest request,
                                               CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var result = await _loginService.HandleLoginAsync(sessionId, request, ct);

        return ProcessHandleResult(result, (successResult) =>
        {
            return successResult switch
            {
                LoginRedirect r => Ok(r.RedirectResponse),
                LoginTokenSuccess r => Ok(r.TokenResponse),
                _ => StatusCode(500)
            };
        });

    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { message = "Missing refresh token" });

        await _loginService.HandleLogoutAsync(request.Token, ct);

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
