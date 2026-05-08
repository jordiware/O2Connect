using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/authorize")]
public class AuthorizationController : ControllerBase
{
    [HttpGet]
    public IActionResult Authorize()
    {
        return Ok("/connect/authorize endpoint");
    }
}
