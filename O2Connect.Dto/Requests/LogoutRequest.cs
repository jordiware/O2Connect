using System.Text.Json.Serialization;

namespace O2Connect.Dto.Requests;

public sealed record LogoutRequest
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }

    [JsonPropertyName("token_type_hint")]
    public string? TokenTypeHint { get; init; }

    [JsonPropertyName("id_token_hint")]
    public string? IdTokenHint { get; init; }

    [JsonPropertyName("post_logout_redirect_uri")]
    public string? PostLogoutRedirectUri { get; init; }

    [JsonPropertyName("state")]
    public string? State { get; init; }
}
