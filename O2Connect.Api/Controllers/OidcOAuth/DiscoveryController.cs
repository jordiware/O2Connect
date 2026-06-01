using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Crypto;

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
        var jwks = _signingKeyProvider.GetValidSigningKeys()
                                      .Select(k => k.ToJwk())
                                      .ToArray();

        Response.Headers.CacheControl = "public,max-age=300,stale-while-revalidate=3600";

        return Ok(new { keys = jwks });
    }
}
