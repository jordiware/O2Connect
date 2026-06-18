using O2Connect.Dto.OidcOAuth.Connect;

namespace O2Connect.Api.DataValidators;

public static class AuthorizationRequestValidator
{
    public static bool IsPopulated(this AuthorizationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId))
            return false;

        if (string.IsNullOrWhiteSpace(request.RedirectUri))
            return false;

        if (string.IsNullOrWhiteSpace(request.ResponseType))
            return false;

        return true;
    }
}
