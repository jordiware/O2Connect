using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Clients;

public sealed record ClientDetailsResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("owner_id")]
    public required string OwnerId { get; init; }

    [JsonPropertyName("created_at")]
    public required DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("last_modified_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? LastModifiedAt { get; init; }

    [JsonPropertyName("revoked_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? RevokedAt { get; init; }

    [JsonPropertyName("redirect_uris")]
    public required IReadOnlyList<string> RedirectUris { get; init; }

    [JsonPropertyName("allowed_grant_types")]
    public required IReadOnlyList<string> AllowedGrantTypes { get; init; }

    [JsonPropertyName("allowed_scopes")]
    public required IReadOnlyList<string> AllowedScopes { get; init; }

    [JsonPropertyName("allowed_authentication_methods")]
    public required IReadOnlyList<string> AllowedAuthenticationMethods { get; init; }

    [JsonPropertyName("allowed_response_types")]
    public required IReadOnlyList<string> AllowedResponseTypes { get; init; }
}
