using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.DataValidators;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Mappers;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect")]
public class ConnectAuthorizationController : ControllerBase
{
    private readonly IConnectAuthorizationService _authorizationService;
    private readonly IParAuthorizationService _parAuthorizationService;

    public ConnectAuthorizationController(
        IConnectAuthorizationService authorizationService,
        IParAuthorizationService parAuthorizationService)
    {
        _authorizationService = authorizationService;
        _parAuthorizationService = parAuthorizationService;
    }

    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize([FromQuery] AuthorizationRequest? request,
                                               [FromQuery(Name = "request_uri")] string? requestUri,
                                               CancellationToken ct)
    {
        var hasRequest = request != null && request.IsPopulated();
        var hasRequestUri = !string.IsNullOrWhiteSpace(requestUri);

        if (!hasRequest && !hasRequestUri)
            throw OAuthException.FromInvalidRequest();

        if (hasRequest && hasRequestUri)
            throw OAuthException.FromInvalidRequest();

        if (hasRequest)
        {
            var requestData = request!.ToData();
            var result = await _authorizationService.HandleAuthorizationAsync(requestData, User, ct);

            return BuildAuthorizationRedirectResult(result);
        }
        else
        {
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
    }

    [HttpGet("authorize/resume")]
    public async Task<IActionResult> Resume([FromQuery(Name = "session")] string sessionId,
                                            CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        if (string.IsNullOrWhiteSpace(sessionId))
            throw OAuthException.FromInvalidRequest();

        var result = await _authorizationService.HandleSessionAsync(sessionId, User, ct);

        return BuildAuthorizationRedirectResult(result);
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
