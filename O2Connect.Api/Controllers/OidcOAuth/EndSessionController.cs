using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/logout")]
public sealed class EndSessionController : ControllerBase
{
    private readonly IEndSessionService _endSessionService;

    public EndSessionController(IEndSessionService endSessionService)
    {
        _endSessionService = endSessionService;
    }

    [HttpGet]
    public async Task<IActionResult> Logout([FromQuery] EndSessionRequest request)
    {
        await _endSessionService.HandleAsync(request, HttpContext.RequestAborted);

        return NoContent();
    }
}
