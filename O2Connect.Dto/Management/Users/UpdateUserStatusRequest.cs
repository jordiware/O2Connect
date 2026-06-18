using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Users;

public sealed record UpdateUserStatusRequest
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }
}
