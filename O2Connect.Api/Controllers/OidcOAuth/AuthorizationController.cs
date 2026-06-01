using Microsoft.AspNetCore.Mvc;
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
        var result = await _authorizationService.HandleAsync(request, User);

        if (!result.IsRedirect)
        {
            return Redirect($"{request.RedirectUri}?error={result.Error}&error_description={result.ErrorDescription}&state={request.State}");
        }

        var url = $"{result.RedirectUri}?code={result.Code}&state={result.State}";

        return Redirect(url);
    }
}
