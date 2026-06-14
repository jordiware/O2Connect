namespace O2Connect.Api.Repositories.Filters;

public sealed record ClientListQuery(
    int Page,
    int PageSize,
    string SortBy,
    string Order
);
