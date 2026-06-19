using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Profile;

public sealed record UpdateDisplayNameRequest
{
    [JsonPropertyName("display_name")]
    public required string DisplayName { get; init; }
}
