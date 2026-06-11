using System.Text.Json.Serialization;

namespace O2Connect.Dto.Requests;

public sealed record PushedAuthorizationRequest
{
    [JsonPropertyName("client_id")]
    public required string ClientId { get; init; }

    [JsonPropertyName("redirect_uri")]
    public required string RedirectUri { get; init; }

    [JsonPropertyName("response_type")]
    public required string ResponseType { get; init; }

    [JsonPropertyName("scope")]
    public required string Scope { get; init; }

    [JsonPropertyName("code_challenge")]
    public required string CodeChallenge { get; init; }

    [JsonPropertyName("code_challenge_method")]
    public required string CodeChallengeMethod { get; init; }

    [JsonPropertyName("state")]
    public string? State { get; init; }
}
