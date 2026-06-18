using O2Connect.Api.Models.Mappers;
using O2Connect.Api.Repositories.Filters;
using O2Connect.Dto.Management;

namespace O2Connect.Api.DataValidators;

public interface IPaginationQueryValidator
{
    bool ValidatePaginationRequest(QueryPaginationRequest request, out string errorMessage);
    bool ValidatePaginationRequest(JsonPaginationRequest request, out string errorMessage);
}

public class PaginationQueryValidator : IPaginationQueryValidator
{
    public bool ValidatePaginationRequest(QueryPaginationRequest request, out string errorMessage)
    {
        return ValidatePagination(request.ToEntityPagination(), out errorMessage);
    }

    public bool ValidatePaginationRequest(JsonPaginationRequest request, out string errorMessage)
    {
        return ValidatePagination(request.ToEntityPagination(), out errorMessage);
    }

    private static bool ValidatePagination(EntityPagination pagination, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (pagination.Page < 1)
        {
            errorMessage = "Page must be greater than or equal to 1.";
            return false;
        }

        if (pagination.PageSize < 10 || pagination.PageSize > 100)
        {
            errorMessage = "Page size must be between 10 and 100.";
            return false;
        }

        if (!string.IsNullOrEmpty(pagination.Order) && pagination.Order != "asc" && pagination.Order != "desc")
        {
            errorMessage = "Order must be either 'asc' or 'desc'.";
            return false;
        }

        return true;
    }
}
