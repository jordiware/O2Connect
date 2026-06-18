using System.Text.Json.Serialization;

namespace O2Connect.Dto.OidcOAuth.Consent;

public sealed record ConsentResponse
{
    [JsonPropertyName("session_id")]
    public required string SessionId { get; init; }

    [JsonPropertyName("client_id")]
    public required string ClientId { get; init; }

    [JsonPropertyName("client_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClientName { get; init; }

    [JsonPropertyName("user_display_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserDisplayName { get; init; }

    [JsonPropertyName("scope")]
    public required string Scope { get; init; }
}
