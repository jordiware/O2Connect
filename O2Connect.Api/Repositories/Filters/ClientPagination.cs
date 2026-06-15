namespace O2Connect.Api.Repositories.Filters;

public sealed record ClientPagination
{
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required string SortBy { get; init; }
    public required string Order { get; init; }
}
