using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect")]
public sealed class ConnectController : OidcOAuthControllerBase
{
    private readonly IAccountService _userSessionService;
    private readonly IPushedAuthorizationService _pushedAuthorizationService;

    public ConnectController(
        IAccountService userSessionService,
        IPushedAuthorizationService pushedAuthorizationService)
    {
        _userSessionService = userSessionService;
        _pushedAuthorizationService = pushedAuthorizationService;
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout([FromQuery] EndSessionRequest request)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        await _userSessionService.HandleLogoutAsync(request, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpPost("par")]
    public async Task<ActionResult<PushedAuthorizationResponse>> Par(
        [FromBody] PushedAuthorizationRequest request)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var result = await _pushedAuthorizationService.HandleAsync(request, HttpContext.RequestAborted);

        return Ok(result);
    }
}
