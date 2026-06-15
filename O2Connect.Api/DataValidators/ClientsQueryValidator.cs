using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Management.Clients;

namespace O2Connect.Api.DataValidators;

public interface IClientsQueryValidator
{
    bool ValidateListRequest(ClientsPaginationRequest request, out string errorMessage);
    bool ValidateSearchRequest(ClientSearchRequest request, out string errorMessage);
}

public class ClientsQueryValidator : IClientsQueryValidator
{
    public bool ValidateListRequest(ClientsPaginationRequest request, out string errorMessage)
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

    public bool ValidateSearchRequest(ClientSearchRequest request, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (request.Status != null 
            && request.Status.Any(s => !Enum.TryParse<EntityStatus>(s, true, out _)))
        {
            errorMessage = "Invalid status value provided.";
            return false;
        }

        if (request.MinCreatedAt != null 
            && request.MaxCreatedAt != null 
            && request.MinCreatedAt > request.MaxCreatedAt)
        {
            errorMessage = "MinCreatedAt cannot be greater than MaxCreatedAt.";
            return false;
        }

        if (request.MinLastModifiedAt != null 
            && request.MaxLastModifiedAt != null 
            && request.MinLastModifiedAt > request.MaxLastModifiedAt)
        {
            errorMessage = "MinLastModifiedAt cannot be greater than MaxLastModifiedAt.";
            return false;
        }

        if (request.MinRevokedAt != null
            && request.MaxRevokedAt != null
            && request.MinRevokedAt > request.MaxRevokedAt)
        {
            errorMessage = "MinRevokedAt cannot be greater than MaxRevokedAt.";
            return false;
        }

        if (request.GrantTypes != null 
            && request.GrantTypes.Any(gt => !GrantType.TryParse(gt, out _)))
        {
            errorMessage = "GrantTypes cannot invalid values.";
            return false;
        }

        if (request.Scopes != null
            && request.Scopes.Any(s => !Scopes.All.Contains(s)))
        {
            errorMessage = "Scopes cannot contain invalid values.";
            return false;
        }

        if (request.AuthenticationMethods != null
            && request.AuthenticationMethods.Any(am => !ClientAuthenticationMethod.TryParse(am, out _)))
        {
            errorMessage = "AuthenticationMethods cannot contain invalid values.";
            return false;
        }

        return true;
    }
}
