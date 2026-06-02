using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using O2Connect.Api.Models;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/authorize")]
public class AuthorizationController : OidcOAuthControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationController(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<IActionResult> Authorize([FromQuery] AuthorizationRequest request)
    {
        var result = await _authorizationService.HandleAsync(request, User, HttpContext.RequestAborted);
        return BuildRedirect(result);
    }

    [HttpGet("resume/{sessionId}")]
    public async Task<IActionResult> Resume(string sessionId, CancellationToken ct)
    {
        var result = await _authorizationService.AuthorizeAsync(sessionId, User, ct);
        return BuildRedirect(result);
    }

    private IActionResult BuildRedirect(AuthorizationResult result)
    {
        if (!result.Success)
        {
            var errorUrl = QueryHelpers.AddQueryString(result.RedirectUri, new Dictionary<string, string?>
            {
                ["error"] = result.Error,
                ["error_description"] = result.ErrorDescription,
                ["state"] = result.State
            });

            return Redirect(errorUrl);
        }

        var url = QueryHelpers.AddQueryString(result.RedirectUri, new Dictionary<string, string?>
        {
            ["code"] = result.Code,
            ["state"] = result.State
        });

        return Redirect(url);
    }
}
