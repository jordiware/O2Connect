using Microsoft.AspNetCore.Mvc;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/authorize")]
public class AuthorizationController : ControllerBase
{
    [HttpGet]
    public IActionResult Authorize([FromQuery] AuthorizationRequest request)
    {
        return Ok("/connect/authorize endpoint");
    }
}
