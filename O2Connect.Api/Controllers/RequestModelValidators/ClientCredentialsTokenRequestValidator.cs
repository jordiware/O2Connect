using O2Connect.Api.DtoMappers;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Context;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.RequestModelValidators;

public class ClientCredentialsTokenRequestValidator : ITokenRequestValidator
{
    public GrantType GrantType => GrantType.ClientCredentials;

    public TokenRequestContext Validate(TokenRequest request, Client client, ClientAuthenticationMethod method)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId))
            throw OAuthException.FromInvalidRequest("Missing 'client_id'.");

        return request.ToRequestContext(client, method);
    }
}
