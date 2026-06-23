using O2Connect.Api.Config;
using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Dto.OidcOAuth.Discovery;

namespace O2Connect.Api.Services.OidcOAuth;

public interface IDiscoveryMetadataService
{
    OAuthAuthorizationServerMetadataResponse GetOAuthAuthorizationServer();
    OpenIdProviderMetadataResponse GetOpenIdConfiguration();
}

public sealed class DiscoveryMetadataService : IDiscoveryMetadataService
{
    private readonly IJwtConfig _jwtConfig;
    private readonly IDiscoveryEndpointsConfig _endpoints;

    private Uri IssuerUri { get; }

    public DiscoveryMetadataService(
        IJwtConfig jwtConfig,
        IDiscoveryEndpointsConfig endpoints)
    {
        _jwtConfig = jwtConfig;
        _endpoints = endpoints;

        IssuerUri = new Uri(_jwtConfig.Issuer.TrimEnd('/'));
    }

    private DiscoveryMetadata BaseMetadata()
    {
        return new DiscoveryMetadata
        {
            Issuer = IssuerUri.ToString(),

            AuthorizationEndpoint = new Uri(IssuerUri, "/connect/authorize").ToString(),
            TokenEndpoint = new Uri(IssuerUri, "/connect/token").ToString(),
            JwksUri = new Uri(IssuerUri, "/.well-known/jwks.json").ToString(),

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
            RevocationEndpoint = new Uri(IssuerUri, "/connect/revocation").ToString(),
            IntrospectionEndpoint = new Uri(IssuerUri, "/connect/introspect").ToString(),

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

    public OpenIdProviderMetadataResponse GetOpenIdConfiguration()
    {
        var m = BaseMetadata() with
        {
            UserInfoEndpoint = new Uri(IssuerUri, "/connect/userinfo").ToString(),
            EndSessionEndpoint = new Uri(IssuerUri, "/connect/logout").ToString(),

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

        return new OpenIdProviderMetadataResponse
        {
            Issuer = m.Issuer,

            AuthorizationEndpoint = m.AuthorizationEndpoint,
            TokenEndpoint = m.TokenEndpoint,
            UserInfoEndpoint = m.UserInfoEndpoint,
            EndSessionEndpoint = m.EndSessionEndpoint,
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
