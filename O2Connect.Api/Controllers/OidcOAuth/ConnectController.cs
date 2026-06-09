using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.Security.Claims;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect")]
public sealed class ConnectController : ControllerBase
{
    private readonly IAccountService _userSessionService;
    private readonly IPushedAuthorizationService _pushedAuthorizationService;
    private readonly ITokenIntrospectionService _tokenIntrospectionService;
    private readonly IUserInfoService _userInfoService;

    public ConnectController(
        IAccountService userSessionService,
        IPushedAuthorizationService pushedAuthorizationService,
        ITokenIntrospectionService tokenIntrospectionService,
        IUserInfoService userInfoService)
    {
        _userSessionService = userSessionService;
        _pushedAuthorizationService = pushedAuthorizationService;
        _tokenIntrospectionService = tokenIntrospectionService;
        _userInfoService = userInfoService;
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout([FromQuery] EndSessionRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        await _userSessionService.HandleLogoutAsync(request, ct);

        return NoContent();
    }

    [HttpPost("par")]
    public async Task<ActionResult<PushedAuthorizationResponse>> Par(
        [FromBody] PushedAuthorizationRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var result = await _pushedAuthorizationService.HandleAsync(request, ct);

        return Ok(result);
    }

    [HttpPost("introspect")]
    [Authorize(AuthenticationSchemes = "Client")]
    public async Task<IActionResult> Introspect([FromForm] string token, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        if (string.IsNullOrWhiteSpace(token))
            return Ok(IntrospectionResponse.Inactive);

        var clientId = User.FindFirstValue("client_id");

        if (string.IsNullOrWhiteSpace(clientId))
            return Ok(IntrospectionResponse.Inactive);

        var result = await _tokenIntrospectionService.IntrospectAsync(token, clientId, ct);

        return Ok(result);
    }

    [HttpGet("userinfo")]
    public async Task<IActionResult> UserInfoGet(CancellationToken ct)
    {
        var result = await _userInfoService.GetUserInfoAsync(User, ct);

        return Ok(result);
    }

    [HttpPost("userinfo")]
    public Task<IActionResult> UserInfoPost(CancellationToken ct) => UserInfoGet(ct);
}
