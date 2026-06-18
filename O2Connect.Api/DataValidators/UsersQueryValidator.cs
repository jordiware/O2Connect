using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Management;
using O2Connect.Dto.Management.Users;

namespace O2Connect.Api.DataValidators;

public interface IUsersQueryValidator
{
    bool ValidateSearchRequest(UsersSearchRequest request, out string errorMessage);
}

public class UsersQueryValidator : IUsersQueryValidator
{
    public bool ValidateSearchRequest(UsersSearchRequest request, out string errorMessage)
    {
        errorMessage = string.Empty;

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
