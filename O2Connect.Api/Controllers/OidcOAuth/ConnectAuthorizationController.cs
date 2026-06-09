using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/authorize")]
public class ConnectAuthorizationController : ControllerBase
{
    private readonly IAuthorizeService _authorizationService;
    private readonly IParAuthorizationService _parAuthorizationService;

    public ConnectAuthorizationController(
        IAuthorizeService authorizationService,
        IParAuthorizationService parAuthorizationService)
    {
        _authorizationService = authorizationService;
        _parAuthorizationService = parAuthorizationService;
    }

    [HttpGet]
    public async Task<IActionResult> Authorize([FromQuery] AuthorizationRequest request,
                                               CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var result = await _authorizationService.ProcessAuthorizationAsync(request, User, ct);

        return BuildAuthorizationRedirectResult(result);
    }

    [HttpGet("resume")]
    public async Task<IActionResult> Resume([FromQuery(Name = "session")] string sessionId,
                                            CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        if (string.IsNullOrWhiteSpace(sessionId))
            throw OAuthException.FromInvalidRequest();

        var result = await _authorizationService.ProcessSessionAsync(sessionId, User, ct);

        return BuildAuthorizationRedirectResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> Authorize([FromQuery(Name = "request_uri")] string requestUri,
                                               CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        if (string.IsNullOrWhiteSpace(requestUri))
            throw OAuthException.FromInvalidRequest();

        var result = await _parAuthorizationService.HandleAsync(requestUri, HttpContext, ct);

        if (result is null
            || string.IsNullOrWhiteSpace(result.Action)
            || !string.Equals("redirect", result.Action, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(result.RedirectUrl))
            throw OAuthException.FromServerError();

        return Redirect(result.RedirectUrl);
    }

    private IActionResult BuildAuthorizationRedirectResult(AuthorizationResult result)
    {
        if (!Uri.TryCreate(result.RedirectUri, UriKind.Absolute, out var baseUri))
            return BadRequest("Invalid redirect URI");

        var parameters = new Dictionary<string, string?>
        {
            ["state"] = result.State
        };

        if (result.Success)
        {
            parameters["code"] = result.Code;
        }
        else
        {
            parameters["error"] = result.Error;
            parameters["error_description"] = result.ErrorDescription;
        }

        var paramsString = QueryString.Create(parameters).ToString().TrimStart('?', '#');

        var separator = result.ResponseMode switch
        {
            AuthorizationResultResponseMode.Query => "?",
            AuthorizationResultResponseMode.Fragment => "#",
            _ => throw new ArgumentException(),
        };

        var url = baseUri + separator + paramsString;

        return Redirect(url);
    }
}
