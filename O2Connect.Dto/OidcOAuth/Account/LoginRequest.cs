using System.Text.Json.Serialization;

namespace O2Connect.Dto.OidcOAuth.Account;

public sealed record LoginRequest
{
    [JsonPropertyName("username")]
    public required string Username { get; init; }

    [JsonPropertyName("password")]
    public required string Password { get; init; }

    [JsonPropertyName("remember_me")]
    public required bool RememberMe { get; init; }

    [JsonPropertyName("client_id")]
    public required string ClientId { get; init; }
}
