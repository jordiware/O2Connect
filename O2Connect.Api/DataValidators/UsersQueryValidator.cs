using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Management;
using O2Connect.Dto.Management.Users;

namespace O2Connect.Api.DataValidators;

public interface IUsersQueryValidator
{
    bool ValidatePaginationRequest(QueryPaginationRequest request, out string errorMessage);
    bool ValidatePaginationRequest(JsonPaginationRequest request, out string errorMessage);
    bool ValidateSearchRequest(UsersSearchRequest request, out string errorMessage);
}

public class UsersQueryValidator : IUsersQueryValidator
{
    public bool ValidatePaginationRequest(QueryPaginationRequest request, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (request.Page < 1)
        {
            errorMessage = "Page must be greater than or equal to 1.";
            return false;
        }

        if (request.PageSize < 10 || request.PageSize > 100)
        {
            errorMessage = "Page size must be between 10 and 100.";
            return false;
        }

        if (!string.IsNullOrEmpty(request.Order) && request.Order != "asc" && request.Order != "desc")
        {
            errorMessage = "Order must be either 'asc' or 'desc'.";
            return false;
        }

        return true;
    }

    public bool ValidatePaginationRequest(JsonPaginationRequest request, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (request.Page < 1)
        {
            errorMessage = "Page must be greater than or equal to 1.";
            return false;
        }

        if (request.PageSize < 10 || request.PageSize > 100)
        {
            errorMessage = "Page size must be between 10 and 100.";
            return false;
        }

        if (!string.IsNullOrEmpty(request.Order) && request.Order != "asc" && request.Order != "desc")
        {
            errorMessage = "Order must be either 'asc' or 'desc'.";
            return false;
        }

        return true;
    }

    public bool ValidateSearchRequest(UsersSearchRequest request, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (!ValidatePaginationRequest(request.Pagination, out errorMessage))
        {
            return false;
        }

        var filterRequest = request.Filter;

        if (filterRequest.Role != null
            && !UserRole.TryParse(filterRequest.Role, out _))
        {
            errorMessage = "Invalid role value provided.";
            return false;
        }

        if (filterRequest.Status != null 
            && filterRequest.Status.Any(s => !Enum.TryParse<EntityStatus>(s, true, out _)))
        {
            errorMessage = "Invalid status value provided.";
            return false;
        }

        if (filterRequest.MinCreatedAt != null 
            && filterRequest.MaxCreatedAt != null 
            && filterRequest.MinCreatedAt > filterRequest.MaxCreatedAt)
        {
            errorMessage = "MinCreatedAt cannot be greater than MaxCreatedAt.";
            return false;
        }

        if (filterRequest.MinLastModifiedAt != null 
            && filterRequest.MaxLastModifiedAt != null 
            && filterRequest.MinLastModifiedAt > filterRequest.MaxLastModifiedAt)
        {
            errorMessage = "MinLastModifiedAt cannot be greater than MaxLastModifiedAt.";
            return false;
        }

        if (filterRequest.MinRevokedAt != null
            && filterRequest.MaxRevokedAt != null
            && filterRequest.MinRevokedAt > filterRequest.MaxRevokedAt)
        {
            errorMessage = "MinRevokedAt cannot be greater than MaxRevokedAt.";
            return false;
        }

        return true;
    }
}
