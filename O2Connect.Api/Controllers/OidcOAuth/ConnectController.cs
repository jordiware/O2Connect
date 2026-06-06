using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect")]
public sealed class ConnectController : ControllerBase
{
    private readonly IEndSessionService _endSessionService;

    public ConnectController(IEndSessionService endSessionService)
    {
        _endSessionService = endSessionService;
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout([FromQuery] EndSessionRequest request)
    {
        await _endSessionService.HandleAsync(request, HttpContext.RequestAborted);

        return NoContent();
    }
}
