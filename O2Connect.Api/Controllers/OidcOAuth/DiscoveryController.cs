using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route(".well-known")]
public class DiscoveryController : ControllerBase
{
    [HttpGet("openid-configuration")]
    public IActionResult OpenIdConfiguration()
    {
        return Ok("/.well-known/openid-configuration endpoint");
    }

    [HttpGet("jwks.json")]
    public IActionResult Jwks()
    {
        return Ok("/.well-known/jwks.json endpoint");
    }
}
