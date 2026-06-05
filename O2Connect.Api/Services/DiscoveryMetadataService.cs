using Microsoft.Extensions.Options;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Options;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services;

public interface IDiscoveryMetadataService
{
    OAuthAuthorizationServerMetadataResponse GetOAuthAuthorizationServer();
    OpenIdProviderMetadataResponse GetOpenIdConfiguration();
}

public sealed class DiscoveryMetadataService : IDiscoveryMetadataService
{
    private readonly JwtOptions _jwtOptions;
    private readonly DiscoveryEndpoints _endpoints;

    public DiscoveryMetadataService(
        IOptions<JwtOptions> jwtOptions,
        IOptions<DiscoveryEndpoints> endpoints)
    {
        _jwtOptions = jwtOptions.Value;
        _endpoints = endpoints.Value;
    }

    private Uri BaseUri()
    {
        var issuer = _jwtOptions.Issuer.TrimEnd('/');
        return new Uri(issuer);
    }

    private DiscoveryMetadata BaseMetadata()
    {
        var issuerUri = BaseUri();

        return new DiscoveryMetadata
        {
            Issuer = issuerUri.ToString(),

            AuthorizationEndpoint = new Uri(issuerUri, "/connect/authorize").ToString(),
            TokenEndpoint = new Uri(issuerUri, "/connect/token").ToString(),
            JwksUri = new Uri(issuerUri, "/.well-known/jwks.json").ToString(),

            GrantTypesSupported = GrantType.Supported
                .Select(x => x.Value)
                .Order(StringComparer.Ordinal)
                .ToArray(),

            ResponseTypesSupported = ["code"],
            ResponseModesSupported = ["query", "form_post"],

            TokenEndpointAuthMethodsSupported = ClientAuthenticationMethod.Supported
                .Select(x => x.Value)
                .Order(StringComparer.Ordinal)
                .ToArray(),

            CodeChallengeMethodsSupported = PkceMethod.Supported
                .Select(x => x.Value)
                .Order(StringComparer.Ordinal)
                .ToArray(),

            ServiceDocumentation = _endpoints.Documentation,
            PolicyUri = _endpoints.PrivacyPolicy,
            TosUri = _endpoints.TermsOfService
        };
    }

    public OAuthAuthorizationServerMetadataResponse GetOAuthAuthorizationServer()
    {
        var m = BaseMetadata() with
        {
            RevocationEndpoint = new Uri(BaseUri(), "/connect/revocation").ToString(),
            IntrospectionEndpoint = new Uri(BaseUri(), "/connect/introspect").ToString(),

            ScopesSupported =
            [
                "openid",
                "profile",
                "email",
                "offline_access",
                "api.read",
                "api.write"
            ]
        };

        return MapOAuthAuthorizationServerMetadata(m);
    }

    public OpenIdProviderMetadataResponse GetOpenIdConfiguration()
    {
        var m = BaseMetadata() with
        {
            UserInfoEndpoint = new Uri(BaseUri(), "/connect/userinfo").ToString(),

            IdTokenSigningAlgValuesSupported = "RS256",

            ScopesSupported =
            [
                "openid",
                "profile",
                "email"
            ],

            ClaimsSupported =
            [
                "sub",
                "email",
                "name",
                "preferred_username",
                "given_name",
                "family_name"
            ]
        };

        return MapOpenIdProviderMetadata(m);
    }

    public static OAuthAuthorizationServerMetadataResponse MapOAuthAuthorizationServerMetadata(DiscoveryMetadata m)
    {
        return new OAuthAuthorizationServerMetadataResponse
        {
            Issuer = m.Issuer,
            AuthorizationEndpoint = m.AuthorizationEndpoint,
            TokenEndpoint = m.TokenEndpoint,
            JwksUri = m.JwksUri,

            ResponseTypesSupported = m.ResponseTypesSupported,
            ResponseModesSupported = m.ResponseModesSupported,

            GrantTypesSupported = m.GrantTypesSupported,
            TokenEndpointAuthMethodsSupported = m.TokenEndpointAuthMethodsSupported,

            RevocationEndpoint = m.RevocationEndpoint,
            IntrospectionEndpoint = m.IntrospectionEndpoint,

            CodeChallengeMethodsSupported = m.CodeChallengeMethodsSupported,

            ScopesSupported = m.ScopesSupported,

            ServiceDocumentation = m.ServiceDocumentation,
            OpPolicyUri = m.PolicyUri,
            OpTosUri = m.TosUri
        };
    }

    public static OpenIdProviderMetadataResponse MapOpenIdProviderMetadata(DiscoveryMetadata m)
    {
        return new OpenIdProviderMetadataResponse
        {
            Issuer = m.Issuer,

            AuthorizationEndpoint = m.AuthorizationEndpoint,
            TokenEndpoint = m.TokenEndpoint,
            UserInfoEndpoint = m.UserInfoEndpoint,
            JwksUri = m.JwksUri,

            ResponseTypesSupported = m.ResponseTypesSupported,
            ResponseModesSupported = m.ResponseModesSupported,

            SubjectTypesSupported = ["public"],

            IdTokenSigningAlgValuesSupported = ["RS256"],

            GrantTypesSupported = m.GrantTypesSupported,
            TokenEndpointAuthMethodsSupported = m.TokenEndpointAuthMethodsSupported,

            CodeChallengeMethodsSupported = m.CodeChallengeMethodsSupported,

            ScopesSupported = m.ScopesSupported,
            ClaimsSupported = m.ClaimsSupported,

            ServiceDocumentation = m.ServiceDocumentation,
            OpPolicyUri = m.PolicyUri,
            OpTosUri = m.TosUri
        };
    }
}
