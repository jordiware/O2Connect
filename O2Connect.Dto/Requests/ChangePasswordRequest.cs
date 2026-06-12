using System.Text.Json.Serialization;

namespace O2Connect.Dto.Requests;

public sealed record ChangePasswordRequest
{
    [JsonPropertyName("current_password")]
    public required string CurrentPassword { get; init; }

    [JsonPropertyName("new_password")]
    public required string NewPassword { get; init; }
}
