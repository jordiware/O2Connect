using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/token")]
public class TokenController : ControllerBase
{
    [HttpPost]
    public IActionResult Token()
    {
        return Ok("/connect/token endpoint");
    }
}
