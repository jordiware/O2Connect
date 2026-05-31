using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Context;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.RequestModelValidators;

public class RefreshTokenTokenRequestValidator : ITokenRequestValidator
{
    public GrantType GrantType => GrantType.RefreshToken;

    public async Task<TokenRequestContext> ValidateAsync(TokenRequest request,
                                                         Client client,
                                                         ClientAuthenticationMethod method,
                                                         CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId))
            throw OAuthException.FromInvalidRequest("Missing 'client_id'.");

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw OAuthException.FromInvalidRequest("Missing 'redirect_uri'.");

        return default!; // TODO
    }
}
