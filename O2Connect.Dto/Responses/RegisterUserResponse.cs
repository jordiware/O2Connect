using System.Text.Json.Serialization;

namespace O2Connect.Dto.Responses;

public sealed record RegisterUserResponse
{
    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }
}
