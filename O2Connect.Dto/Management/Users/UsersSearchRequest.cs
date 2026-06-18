using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Users;

public sealed record UsersSearchRequest
{
    [JsonPropertyName("pagination")]
    public JsonPaginationRequest Pagination { get; init; } = new();

    [JsonPropertyName("filters")]
    public UsersSearchFilterRequest Filters { get; init; } = new();
}

public sealed record UsersSearchFilterRequest
{
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; init; }

    [JsonPropertyName("role")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Role { get; init; }

    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Status { get; init; }

    [JsonPropertyName("min_created_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? MinCreatedAt { get; init; }

    [JsonPropertyName("max_created_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? MaxCreatedAt { get; init; }

    [JsonPropertyName("min_last_modified_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? MinLastModifiedAt { get; init; }

    [JsonPropertyName("max_last_modified_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? MaxLastModifiedAt { get; init; }

    [JsonPropertyName("min_revoked_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? MinRevokedAt { get; init; }

    [JsonPropertyName("max_revoked_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? MaxRevokedAt { get; init; }
}
