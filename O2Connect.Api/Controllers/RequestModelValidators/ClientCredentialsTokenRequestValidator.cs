using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.RequestInputs;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.RequestModelValidators;

public class ClientCredentialsTokenRequestValidator : ITokenRequestValidator
{
    public GrantType GrantType => GrantType.ClientCredentials;

    public TokenRequestInput Validate(TokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId))
            throw OAuthException.FromInvalidRequest("Missing 'client_id'.");

        return TokenRequestInput.FromRequestDto(request);
    }
}
