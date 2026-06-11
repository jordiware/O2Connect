using System.Text.Json.Serialization;

namespace O2Connect.Dto.Responses;

public sealed record RedirectResponse
{
    [JsonPropertyName("action")]
    public required string Action { get; init; }

    [JsonPropertyName("redirect_url")]
    public required string RedirectUrl { get; init; }
}
