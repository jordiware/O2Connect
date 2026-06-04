using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using O2Connect.Api.Crypto;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Options;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route(".well-known")]
public class DiscoveryController : ControllerBase
{
    private readonly JwtOptions _jwtOptions;
    private readonly DiscoveryEndpoints _discoveryEndpoints;
    private readonly ISigningKeyProvider _signingKeyProvider;

    public DiscoveryController(
        IOptions<JwtOptions> jwtOptions,
        IOptions<DiscoveryEndpoints> discoveryEndpoints,
        ISigningKeyProvider signingKeyProvider)
    {
        _jwtOptions = jwtOptions.Value;
        _discoveryEndpoints = discoveryEndpoints.Value;
        _signingKeyProvider = signingKeyProvider;
    }

    [HttpGet("oauth-authorization-server")]
    public IActionResult OAuthAuthorizationServer()
    {
        var issuer = _jwtOptions.Issuer.TrimEnd('/');
        var issuerUri = new Uri(issuer);

        var jwksEndpoint = new Uri(issuerUri, "/.well-known/jwks.json");
        var authorizationEndpoint = new Uri(issuerUri, "/connect/authorize");
        var tokenEndpoint = new Uri(issuerUri, "/connect/token");
        var revocationEndpoint = new Uri(issuerUri, "/connect/revocation");
        var introspectionEndpoint = new Uri(issuerUri, "/connect/introspect");

        var metadata = new
        {
            issuer = issuerUri.ToString(),

            authorization_endpoint = authorizationEndpoint.ToString(),
            token_endpoint = tokenEndpoint.ToString(),
            token_endpoint_auth_signing_alg_values_supported = new[] { "RS256" },
            revocation_endpoint = revocationEndpoint.ToString(),
            introspection_endpoint = introspectionEndpoint.ToString(),
            jwks_uri = jwksEndpoint.ToString(),

            response_types_supported = new[] { "code" },
            response_modes_supported = new[] { "query", "form_post" },

            scopes_supported = new[]
            {
                "openid",
                "profile",
                "email",
                "offline_access",
                "api.read",
                "api.write"
            },

            grant_types_supported = GrantType.Supported
                .Select(t => t.Value)
                .Order(StringComparer.Ordinal)
                .ToArray(),

            token_endpoint_auth_methods_supported =
                ClientAuthenticationMethod.Supported
                    .Select(t => t.Value)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),

            revocation_endpoint_auth_methods_supported =
                ClientAuthenticationMethod.Supported
                    .Select(t => t.Value)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),

            code_challenge_methods_supported =
                PkceMethod.Supported
                    .Select(t => t.Value)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),

            service_documentation = _discoveryEndpoints.Documentation,
            op_policy_uri = _discoveryEndpoints.PrivacyPolicy,
            op_tos_uri = _discoveryEndpoints.TermsOfService
        };

        Response.Headers.CacheControl = "public,max-age=300,stale-while-revalidate=3600";

        return Ok(metadata);
    }

    [HttpGet("openid-configuration")]
    public IActionResult OpenIdConfiguration()
    {
        var issuer = _jwtOptions.Issuer.TrimEnd('/');
        var issuerUri = new Uri(issuer);

        var jwksEndpoint = new Uri(issuerUri, "/.well-known/jwks.json");
        var authorizationEndpoint = new Uri(issuerUri, "/connect/authorize");
        var tokenEndpoint = new Uri(issuerUri, "/connect/token");
        var userinfoEndpoint = new Uri(issuerUri, "/connect/userinfo");

        var config = new
        {
            issuer = issuerUri.ToString(),

            jwks_uri = jwksEndpoint.ToString(),

            authorization_endpoint = authorizationEndpoint.ToString(),
            token_endpoint = tokenEndpoint.ToString(),
            userinfo_endpoint = userinfoEndpoint.ToString(),

            request_uri_parameter_supported = false,
            authorization_response_iss_parameter_supported = false,

            response_types_supported = new[] { "code" },
            response_modes_supported = new[] { "query", "form_post" },
            subject_types_supported = new[] { "public" },
            grant_types_supported = GrantType.Supported.Select(t => t.Value)
                                                       .Order(StringComparer.Ordinal)
                                                       .ToArray(),
            code_challenge_methods_supported = PkceMethod.Supported.Select(t => t.Value)
                                                                   .Order(StringComparer.Ordinal)
                                                                   .ToArray(),
            token_endpoint_auth_methods_supported =
                ClientAuthenticationMethod.Supported.Select(t => t.Value)
                                                    .Order(StringComparer.Ordinal)
                                                    .ToArray(),

            id_token_signing_alg_values_supported = new[] { "RS256" },
            token_endpoint_auth_signing_alg_values_supported = new[] { "RS256" },

            scopes_supported = new[] { "openid", "profile", "email" },
            claims_supported = new[] { "sub", "email", "name", "preferred_username", "given_name", "family_name" },

            service_documentation = _discoveryEndpoints.Documentation,
            op_policy_uri = _discoveryEndpoints.PrivacyPolicy,
            op_tos_uri = _discoveryEndpoints.TermsOfService,
        };

        Response.Headers.CacheControl = "public,max-age=300,stale-while-revalidate=3600";

        return Ok(config);
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
