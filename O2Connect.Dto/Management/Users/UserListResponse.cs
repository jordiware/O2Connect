using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Users;

public sealed record UserListResponse
{
    [JsonPropertyName("items")]
    public required IReadOnlyList<UserSummaryResponse> Items { get; init; }

    [JsonPropertyName("total_items")]
    public required int TotalItems { get; init; }

    [JsonPropertyName("page")]
    public required int Page { get; init; }

    [JsonPropertyName("total_pages")]
    public required int TotalPages { get; init; }

    [JsonPropertyName("page_size")]
    public required int PageSize { get; init; }
}
