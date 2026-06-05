namespace O2Connect.Api.Models;

public sealed record DiscoveryMetadata
{
    public required string Issuer { get; init; }

    public required string AuthorizationEndpoint { get; init; }
    public required string TokenEndpoint { get; init; }
    public required string JwksUri { get; init; }

    public string? UserInfoEndpoint { get; init; }
    public string? RevocationEndpoint { get; init; }
    public string? IntrospectionEndpoint { get; init; }

    public required string[] GrantTypesSupported { get; init; }
    public required string[] ResponseTypesSupported { get; init; }
    public required string[] ResponseModesSupported { get; init; }

    public required string[] TokenEndpointAuthMethodsSupported { get; init; }
    public required string[] CodeChallengeMethodsSupported { get; init; }

    public string[]? ScopesSupported { get; init; }
    public string[]? ClaimsSupported { get; init; }

    public string? IdTokenSigningAlgValuesSupported { get; init; }

    public string? ServiceDocumentation { get; init; }
    public string? PolicyUri { get; init; }
    public string? TosUri { get; init; }
}
