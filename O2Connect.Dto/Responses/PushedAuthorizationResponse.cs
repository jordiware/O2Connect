using System.Text.Json.Serialization;

namespace O2Connect.Dto.Responses;

public sealed record PushedAuthorizationResponse
{
    [JsonPropertyName("request_uri")]
    public required string RequestUri { get; init; }

    [JsonPropertyName("expires_in")]
    public required int ExpiresIn { get; init; }
}
