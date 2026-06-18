using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Users;

public sealed record UpdateUserDisplayNameRequest
{
    [JsonPropertyName("display_name")]
    public required string DisplayName { get; init; }
}
