using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Management.Clients;

namespace O2Connect.Api.DataValidators;

public interface IClientsQueryValidator
{
    bool ValidateSearchRequest(ClientsSearchRequest request, out string errorMessage);
}

public class ClientsQueryValidator : IClientsQueryValidator
{
    public bool ValidateSearchRequest(ClientsSearchRequest request, out string errorMessage)
    {
        errorMessage = string.Empty;

        var filterRequest = request.Filter;

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

        if (filterRequest.GrantTypes != null 
            && filterRequest.GrantTypes.Any(gt => !GrantType.TryParse(gt, out _)))
        {
            errorMessage = "GrantTypes cannot invalid values.";
            return false;
        }

        if (filterRequest.Scopes != null
            && filterRequest.Scopes.Any(s => !Scopes.All.Contains(s)))
        {
            errorMessage = "Scopes cannot contain invalid values.";
            return false;
        }

        if (filterRequest.AuthenticationMethods != null
            && filterRequest.AuthenticationMethods.Any(am => !ClientAuthenticationMethod.TryParse(am, out _)))
        {
            errorMessage = "AuthenticationMethods cannot contain invalid values.";
            return false;
        }

        return true;
    }
}
