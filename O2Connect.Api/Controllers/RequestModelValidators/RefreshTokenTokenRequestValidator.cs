using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.RequestInputs;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.RequestModelValidators;

public class RefreshTokenTokenRequestValidator : ITokenRequestValidator
{
    public GrantType GrantType => GrantType.RefreshToken;

    public TokenRequestInput Validate(TokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId))
            throw OAuthException.FromInvalidRequest("Missing 'client_id'.");

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw OAuthException.FromInvalidRequest("Missing 'redirect_uri'.");

        return TokenRequestInput.FromRequestDto(request);
    }
}
