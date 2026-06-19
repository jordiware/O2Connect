using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Profile;

public sealed record UpdateImageUrlRequest
{
    [JsonPropertyName("image_url")]
    public required string? ImageUrl { get; init; }
}
