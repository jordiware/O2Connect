using System.Text.Json.Serialization;

namespace O2Connect.Dto.OidcOAuth.Discovery;

public sealed record OpenIdProviderMetadataResponse
{
    [JsonPropertyName("issuer")]
    public required string Issuer { get; init; }

    [JsonPropertyName("authorization_endpoint")]
    public required string AuthorizationEndpoint { get; init; }

    [JsonPropertyName("token_endpoint")]
    public required string TokenEndpoint { get; init; }

    [JsonPropertyName("userinfo_endpoint")]
    public required string UserInfoEndpoint { get; init; }

    [JsonPropertyName("end_session_endpoint")]
    public required string EndSessionEndpoint { get; init; }

    [JsonPropertyName("jwks_uri")]
    public required string JwksUri { get; init; }

    [JsonPropertyName("response_types_supported")]
    public required string[] ResponseTypesSupported { get; init; }

    [JsonPropertyName("response_modes_supported")]
    public required string[] ResponseModesSupported { get; init; }

    [JsonPropertyName("subject_types_supported")]
    public required string[] SubjectTypesSupported { get; init; }

    [JsonPropertyName("id_token_signing_alg_values_supported")]
    public required string[] IdTokenSigningAlgValuesSupported { get; init; }

    [JsonPropertyName("grant_types_supported")]
    public required string[] GrantTypesSupported { get; init; }

    [JsonPropertyName("token_endpoint_auth_methods_supported")]
    public required string[] TokenEndpointAuthMethodsSupported { get; init; }

    [JsonPropertyName("code_challenge_methods_supported")]
    public required string[] CodeChallengeMethodsSupported { get; init; }

    [JsonPropertyName("scopes_supported")]
    public string[]? ScopesSupported { get; init; }

    [JsonPropertyName("claims_supported")]
    public string[]? ClaimsSupported { get; init; }

    [JsonPropertyName("service_documentation")]
    public string? ServiceDocumentation { get; init; }

    [JsonPropertyName("op_policy_uri")]
    public string? OpPolicyUri { get; init; }

    [JsonPropertyName("op_tos_uri")]
    public string? OpTosUri { get; init; }
}
