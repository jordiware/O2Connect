using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management;

public sealed record JsonPaginationRequest
{
    [JsonPropertyName("page")]
    public int Page { get; init; } = 1;

    [JsonPropertyName("page_size")]
    public int PageSize { get; init; } = 20;

    [JsonPropertyName("sort_by")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SortBy { get; init; }

    [JsonPropertyName("order")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Order { get; init; }
}
