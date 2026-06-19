using System.Text.Json.Serialization;

namespace O2Connect.Dto.OidcOAuth.Account;

public sealed record MyAccountResponse
{
    [JsonPropertyName("is_authenticated")]
    public required bool IsAuthenticated { get; init; }

    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("username")]
    public required string Username { get; init; }

    [JsonPropertyName("roles")]
    public required string[] Roles { get; init; }
}
