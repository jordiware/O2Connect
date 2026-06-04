using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Models;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/authorize")]
public class AuthorizationController : OidcOAuthControllerBase
{
    private readonly IAuthorizeService _authorizationService;

    public AuthorizationController(IAuthorizeService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<IActionResult> Authorize([FromQuery] AuthorizationRequest request)
    {
        var result = await _authorizationService.ProcessAuthorizationAsync(request, User, HttpContext.RequestAborted);
        return BuildAuthorizationRedirectResult(result);
    }

    [HttpGet("resume/{sessionId}")]
    public async Task<IActionResult> Resume(string sessionId, CancellationToken ct)
    {
        var result = await _authorizationService.ProcessSessionAsync(sessionId, User, ct);
        return BuildAuthorizationRedirectResult(result);
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
