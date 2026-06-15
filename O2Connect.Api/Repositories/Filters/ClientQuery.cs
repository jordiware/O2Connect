namespace O2Connect.Api.Repositories.Filters;

public sealed record ClientQuery(
    int Page,
    int PageSize,
    string SortBy,
    string Order
);
