using System.Text.Json.Serialization;

namespace O2Connect.Dto.Requests;

public sealed record LoginRequest
{
    [JsonPropertyName("username")]
    public string Username { get; init; } = default!;

    [JsonPropertyName("password")]
    public string Password { get; init; } = default!;

    [JsonPropertyName("remember_me")]
    public bool RememberMe { get; init; } = false;

    [JsonPropertyName("client_id")]
    public string ClientId { get; init; } = default!;
}
