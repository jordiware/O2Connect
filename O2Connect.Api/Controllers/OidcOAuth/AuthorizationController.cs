using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/authorize")]
public class AuthorizationController : OidcOAuthControllerBase
{
    private readonly IAuthorizeService _authorizationService;
    private readonly IParAuthorizationService _parAuthorizationService;

    public AuthorizationController(
        IAuthorizeService authorizationService,
        IParAuthorizationService parAuthorizationService)
    {
        _authorizationService = authorizationService;
        _parAuthorizationService = parAuthorizationService;
    }

    [HttpGet]
    public async Task<IActionResult> Authorize([FromQuery] AuthorizationRequest request)
    {
        var result = await _authorizationService.ProcessAuthorizationAsync(request, User, HttpContext.RequestAborted);
        return BuildAuthorizationRedirectResult(result);
    }

    [HttpGet("resume/{sessionId}")]
    public async Task<IActionResult> Resume(string sessionId)
    {
        var result = await _authorizationService.ProcessSessionAsync(sessionId, User, HttpContext.RequestAborted);
        return BuildAuthorizationRedirectResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> Authorize([FromQuery(Name = "request_uri")] string requestUri)
    {
        var result = await _parAuthorizationService.HandleAsync(requestUri,
                                                                     HttpContext,
                                                                     HttpContext.RequestAborted);

        if (result is null
            || string.IsNullOrWhiteSpace(result.Action)
            || !string.Equals("redirect", result.Action, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(result.RedirectUrl))
            throw OAuthException.FromServerError();

        return Redirect(result.RedirectUrl);
    }

    private IActionResult BuildAuthorizationRedirectResult(AuthorizationResult result)
    {
        if (!Uri.TryCreate(result.RedirectUri, UriKind.Absolute, out _))
            return BadRequest("Invalid redirect URI");

        var baseUri = result.RedirectUri;

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
