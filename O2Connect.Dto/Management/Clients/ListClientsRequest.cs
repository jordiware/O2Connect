using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Dto.Management.Clients;

public sealed record ListClientsRequest
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
