using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.RequestInputs;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.RequestModelValidators;

public class AuthorizationCodeTokenRequestValidator : ITokenRequestValidator
{
    public GrantType GrantType => GrantType.AuthorizationCode;

    public TokenRequestInput Validate(TokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId))
            throw OAuthException.FromInvalidRequest("Missing 'client_id'.");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw OAuthException.FromInvalidRequest("Missing 'code'.");

        if (string.IsNullOrWhiteSpace(request.RedirectUri))
            throw OAuthException.FromInvalidRequest("Missing 'redirect_uri'.");

        if (!string.IsNullOrWhiteSpace(request.CodeVerifier) && request.CodeVerifier.Length < 43)
            throw OAuthException.FromInvalidRequest("Invalid 'code_verifier'.");

        return TokenRequestInput.FromRequestDto(request);
    }
}
