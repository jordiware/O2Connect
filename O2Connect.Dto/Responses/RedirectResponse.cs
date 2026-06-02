using System.Text.Json.Serialization;

namespace O2Connect.Dto.Responses;

public sealed record RedirectResponse
{
    [JsonPropertyName("action")]
    public string Action { get; init; } = default!;

    [JsonPropertyName("redirectUrl")]
    public string RedirectUrl { get; init; } = default!;
}
