using System.Text.Json.Serialization;

namespace O2Connect.Dto.Responses;

public sealed record DeviceStatusResponse
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("expires_in")]
    public required int ExpiresIn { get; init; }
}
