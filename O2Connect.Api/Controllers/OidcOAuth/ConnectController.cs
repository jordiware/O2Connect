using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Security;
using O2Connect.Api.Services.OidcOAuth;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.Security.Claims;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect")]
public sealed class ConnectController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IPushedAuthorizationService _pushedAuthorizationService;
    private readonly ITokenIntrospectionService _tokenIntrospectionService;
    private readonly IUserInfoService _userInfoService;
    private readonly IRevocationService _revocationService;
    private readonly IClientRegistrationService _clientRegistrationService;

    public ConnectController(
        IAccountService accountService,
        IPushedAuthorizationService pushedAuthorizationService,
        ITokenIntrospectionService tokenIntrospectionService,
        IUserInfoService userInfoService,
        IRevocationService revocationService,
        IClientRegistrationService clientRegistrationService)
    {
        _accountService = accountService;
        _pushedAuthorizationService = pushedAuthorizationService;
        _tokenIntrospectionService = tokenIntrospectionService;
        _userInfoService = userInfoService;
        _revocationService = revocationService;
        _clientRegistrationService = clientRegistrationService;
    }

    [HttpGet("logout")]
    [RequireUserToken]
    public async Task<IActionResult> Logout([FromQuery] EndSessionRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        await _accountService.HandleLogoutAsync(request, ct);

        return NoContent();
    }

    [HttpPost("par")]
    [RequireScope(Scopes.Clients.Write)]
    [RequireClientToken]
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
    [RequireClientToken]
    [RequireScope(Scopes.Tokens.Introspect)]
    public async Task<IActionResult> Introspect([FromForm] string token, CancellationToken ct)
    {
        if (!Request.HasFormContentType)
            throw OAuthException.FromInvalidRequest();
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
    [RequireUserToken]
    [RequireScope(RequireScopeMode.Any, Scopes.Oidc.Profile, Scopes.Oidc.Email, Scopes.Oidc.Address)]
    public async Task<IActionResult> UserInfoGet(CancellationToken ct)
    {
        var result = await _userInfoService.GetUserInfoAsync(User, ct);

        return Ok(result);
    }

    [HttpPost("userinfo")]
    public Task<IActionResult> UserInfoPost(CancellationToken ct) => UserInfoGet(ct);

    [HttpPost("revocation")]
    [RequireScope(Scopes.Tokens.Revoke)]
    public async Task<IActionResult> Revoke([FromForm] RevocationRequest request, CancellationToken ct)
    {
        if (!Request.HasFormContentType)
            throw OAuthException.FromInvalidRequest();
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var clientId = ExtractClientId(HttpContext);

        await _revocationService.HandleAsync(request, clientId, ct);

        return Ok();
    }

    [HttpPost("register")]
    [RequireScope(Scopes.Clients.Write)]
    [RequireClientToken]
    public async Task<IActionResult> Register([FromBody] ClientRegistrationRequest request,
                                              CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var ownerId = ExtractSubjectId(User);

        var response = await _clientRegistrationService.HandleAsync(request, ownerId, ct);

        return Ok(response);
    }

    private static string? ExtractClientId(HttpContext context)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated != true)
            return null;

        var clientId = user.FindFirst("client_id")?.Value;

        if (!string.IsNullOrWhiteSpace(clientId))
            return clientId;

        var azp = user.FindFirst("azp")?.Value;

        if (!string.IsNullOrWhiteSpace(azp))
            return azp;

        return null;
    }

    private static string ExtractSubjectId(ClaimsPrincipal user)
    {
        if (!user.Identity?.IsAuthenticated ?? true)
            throw OAuthException.FromInvalidToken();

        var sub =
            user.FindFirst("sub")?.Value ??
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(sub))
            throw OAuthException.FromInvalidToken();

        return sub;
    }
}
