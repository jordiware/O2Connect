using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Profile;

public sealed record UpdateUsernameRequest
{
    [JsonPropertyName("username")]
    public required string Username { get; init; }
}
