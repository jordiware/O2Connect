using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Profile;

public sealed record ChangePasswordRequest
{
    [JsonPropertyName("old_password")]
    public required string OldPassword { get; init; }

    [JsonPropertyName("new_password")]
    public required string NewPassword { get; init; }
}
