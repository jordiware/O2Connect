using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/revocation")]
public class RevocationController : ControllerBase
{
    [HttpPost]
    public IActionResult Revoke()
    {
        return Ok("/connect/revocation endpoint");
    }
}
