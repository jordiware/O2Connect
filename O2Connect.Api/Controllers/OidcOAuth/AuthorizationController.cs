using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/authorize")]
public class AuthorizationController : ControllerBase
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

        if (!result.Success)
        {
            return Redirect($"{request.RedirectUri}?error={result.Error}&error_description={result.ErrorDescription}&state={request.State}");
        }

        var url = $"{result.RedirectUri}?code={result.Code}&state={result.State}";

        return Redirect(url);
    }

    [HttpGet("resume/{sessionId}")]
    public async Task<IActionResult> Resume(string sessionId, CancellationToken ct)
    {
        var result = await _authorizationService.AuthorizeAsync(sessionId, User, ct);

        if (!result.Success)
        {
            return Redirect($"{result.RedirectUri}?error={result.Error}&error_description={result.ErrorDescription}&state={result.State}");
        }

        var url = $"{result.RedirectUri}?code={result.Code}&state={result.State}";

        return Redirect(url);
    }
}
