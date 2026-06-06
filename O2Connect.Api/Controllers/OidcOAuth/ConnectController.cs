using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect")]
public sealed class ConnectController : ControllerBase
{
    private readonly IEndSessionService _endSessionService;
    private readonly IPushedAuthorizationService _pushedAuthorizationService;

    public ConnectController(
        IEndSessionService endSessionService,
        IPushedAuthorizationService pushedAuthorizationService)
    {
        _endSessionService = endSessionService;
        _pushedAuthorizationService = pushedAuthorizationService;
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout([FromQuery] EndSessionRequest request)
    {
        await _endSessionService.HandleAsync(request, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpPost("par")]
    public async Task<ActionResult<PushedAuthorizationResponse>> Par(
        [FromBody] PushedAuthorizationRequest request)
    {
        var result = await _pushedAuthorizationService.HandleAsync(request, HttpContext.RequestAborted);
        return Ok(result);
    }
}
