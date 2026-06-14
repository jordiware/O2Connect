using O2Connect.Dto.Management.Clients;

namespace O2Connect.Api.DataValidators;

public interface IClientsQueryValidator
{
    bool ValidateListRequest(ListClientsRequest request, out string errorMessage);
}

public class ClientsQueryValidator : IClientsQueryValidator
{
    public bool ValidateListRequest(ListClientsRequest request, out string errorMessage)
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
}
