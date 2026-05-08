using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/introspect")]
public class IntrospectionController : ControllerBase
{
    [HttpPost]
    public IActionResult Introspect()
    {
        return Ok("/connect/introspect endpoint");
    }
}
