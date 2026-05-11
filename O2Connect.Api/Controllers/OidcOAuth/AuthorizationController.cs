using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.RequestDtos;
using O2Connect.Api.Services;

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
        return await _authorizationService.HandleAsync(request);
    }
}
