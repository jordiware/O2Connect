using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Crypto;
using O2Connect.Api.Models;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route(".well-known")]
public class DiscoveryController : ControllerBase
{
    private readonly ISigningKeyProvider _signingKeyProvider;

    public DiscoveryController(ISigningKeyProvider signingKeyProvider)
    {
        _signingKeyProvider = signingKeyProvider;
    }

    [HttpGet("openid-configuration")]
    public IActionResult OpenIdConfiguration()
    {
        return Ok("/.well-known/openid-configuration endpoint");
    }

    [HttpGet("jwks.json")]
    public IActionResult Jwks()
    {
        var keys = _signingKeyProvider.GetSigningKeys();

        var jwks = keys.Where(k => k.Status != SigningKeyStatus.Expired)
                       .Select(k => k.ToJwk())
                       .ToArray();

        Response.Headers.CacheControl = "public,max-age=3600";

        return Ok(new { keys = jwks });
    }
}
