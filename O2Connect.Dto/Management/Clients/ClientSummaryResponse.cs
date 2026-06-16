using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Clients;

public sealed record ClientSummaryResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }
}
