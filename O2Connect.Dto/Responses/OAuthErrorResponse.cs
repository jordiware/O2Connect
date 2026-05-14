using System.Text.Json.Serialization;

namespace O2Connect.Dto.Responses;

public class OAuthErrorResponse
{
    [JsonPropertyName("error")]
    public string Error { get; init; } = default!;

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; init; }

    [JsonPropertyName("error_uri")]
    public string? ErrorUri { get; init; }
}
