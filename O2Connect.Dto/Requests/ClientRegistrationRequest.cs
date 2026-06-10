using System.Text.Json.Serialization;

namespace O2Connect.Dto.Requests;

public sealed record ClientRegistrationRequest
{
    [JsonPropertyName("redirect_uris")]
    public required string[] RedirectUris { get; init; }

    [JsonPropertyName("token_endpoint_auth_method")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TokenEndpointAuthMethod { get; init; }

    [JsonPropertyName("grant_types")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? GrantTypes { get; init; }

    [JsonPropertyName("response_types")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? ResponseTypes { get; init; }

    [JsonPropertyName("client_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClientName { get; init; }

    [JsonPropertyName("scope")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Scope { get; init; }

    [JsonPropertyName("logo_uri")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LogoUri { get; init; }
}
