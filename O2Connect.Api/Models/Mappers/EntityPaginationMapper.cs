using O2Connect.Api.Repositories.Filters;
using O2Connect.Dto.Management;

namespace O2Connect.Api.Models.Mappers;

public static class EntityPaginationMapper
{
    public static EntityPagination ToEntityPagination(this QueryPaginationRequest request)
    {
        return new EntityPagination
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy ?? "created_at",
            Order = request.Order ?? "desc"
        };
    }

    public static EntityPagination ToEntityPagination(this JsonPaginationRequest request)
    {
        return new EntityPagination
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy ?? "created_at",
            Order = request.Order ?? "desc"
        };
    }
}
