using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Profile;

public sealed record RevokeConsentRequest
{
    [JsonPropertyName("client_id")]
    public required string ClientId { get; init; }
}
