using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Clients;

public sealed record ClientListResponse
{
    [JsonPropertyName("items")]
    public required IReadOnlyList<ClientSummaryDto> Items { get; init; }

    [JsonPropertyName("page")]
    public required int Page { get; init; }

    [JsonPropertyName("page_size")]
    public required int PageSize { get; init; }

    [JsonPropertyName("total")]
    public required int Total { get; init; }
}
