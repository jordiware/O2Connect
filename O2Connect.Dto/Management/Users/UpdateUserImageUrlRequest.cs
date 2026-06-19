using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Users;

public sealed record UpdateUserImageUrlRequest
{
    [JsonPropertyName("image_url")]
    public required string ImageUrl { get; init; }
}
