using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Clients;

public sealed record UpdateClientImageUrlRequest
{
    [JsonPropertyName("image_url")]
    public required string ImageUrl { get; init; }
}
