using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("consent")]
public class ConsentController : ControllerBase
{
    [HttpGet]
    public IActionResult GetConsent()
    {
        return Ok("/consent GET endpoint");
    }

    [HttpPost]
    public IActionResult PostConsent()
    {
        return Ok("/consent POST endpoint");
    }
}
