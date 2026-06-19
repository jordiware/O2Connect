using System.Text.Json.Serialization;

namespace O2Connect.Dto.OidcOAuth.Connect;

public sealed record ClientRegistrationResponse
{
    [JsonPropertyName("client_id")]
    public required string ClientId { get; init; }

    [JsonPropertyName("client_secret")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClientSecret { get; init; }

    [JsonPropertyName("client_id_issued_at")]
    public required long ClientIdIssuedAt { get; init; }

    [JsonPropertyName("client_secret_expires_at")]
    public required long ClientSecretExpiresAt { get; init; }

    [JsonPropertyName("redirect_uris")]
    public required string[] RedirectUris { get; init; }

    [JsonPropertyName("grant_types")]
    public required string[] GrantTypes { get; init; }

    [JsonPropertyName("response_types")]
    public required string[] ResponseTypes { get; init; }

    [JsonPropertyName("token_endpoint_auth_method")]
    public required string TokenEndpointAuthMethod { get; init; }

    [JsonPropertyName("client_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClientName { get; init; }

    [JsonPropertyName("scope")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Scope { get; init; }
}
