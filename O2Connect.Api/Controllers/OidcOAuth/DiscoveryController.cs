using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Crypto;
using O2Connect.Api.Services.OidcOAuth;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route(".well-known")]
public class DiscoveryController : ControllerBase
{
    private readonly IDiscoveryMetadataService _metadataService;
    private readonly ISigningKeyProvider _signingKeyProvider;

    public DiscoveryController(
        IDiscoveryMetadataService metadataService,
        ISigningKeyProvider signingKeyProvider)
    {
        _metadataService = metadataService;
        _signingKeyProvider = signingKeyProvider;
    }

    [HttpGet("oauth-authorization-server")]
    [AllowAnonymous]
    public IActionResult OAuthAuthorizationServer()
    {
        var metadata = _metadataService.GetOAuthAuthorizationServer();
        return OkWithCache(metadata);
    }

    [HttpGet("openid-configuration")]
    [AllowAnonymous]
    public IActionResult OpenIdConfiguration()
    {
        var metadata = _metadataService.GetOpenIdConfiguration();
        return OkWithCache(metadata);
    }

    [HttpGet("jwks.json")]
    [AllowAnonymous]
    public IActionResult Jwks()
    {
        var jwks = _signingKeyProvider.GetValidSigningKeys()
                                      .Select(k => k.ToJwk())
                                      .ToArray();

        return OkWithCache(new { keys = jwks });
    }

    private IActionResult OkWithCache(object obj)
    {
        Response.Headers.CacheControl = "public,max-age=300,stale-while-revalidate=3600";
        return Ok(obj);
    }
}
