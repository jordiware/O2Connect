using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Dto.Management;

public sealed record PaginationRequest
{
    [FromQuery(Name = "page")]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "page_size")]
    public int PageSize { get; init; } = 20;

    [FromQuery(Name = "sort_by")]
    public string? SortBy { get; init; }

    [FromQuery(Name = "order")]
    public string? Order { get; init; }
}
