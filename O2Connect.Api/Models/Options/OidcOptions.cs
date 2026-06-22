namespace O2Connect.Api.Models.Options;

public sealed record OidcOptions
{
    public const string SectionName = "Oidc";

    public required string Issuer { get; init; }
    public required string AuthorizationEndpoint { get; init; }
    public required string TokenEndpoint { get; init; }
    public required string UserInfoEndpoint { get; init; }
    public required string JwksEndpoint { get; init; }
    public required string EndSessionEndpoint { get; init; }
    public required IReadOnlyCollection<string> ScopesSupported { get; init; }
    public required IReadOnlyCollection<string> ResponseTypesSupported { get; init; }
}
