using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Context;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Requests;
using System.Collections.Immutable;

namespace O2Connect.Api.Controllers.RequestModelValidators;

public class ClientCredentialsTokenRequestValidator : ITokenRequestValidator
{
    public GrantType GrantType => GrantType.ClientCredentials;

    public Task<TokenRequestContext> ValidateAsync(TokenRequest request,
                                                   Client client,
                                                   ClientAuthenticationMethod method,
                                                   CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!client.AllowedGrantTypes.Contains(GrantType.Value, StringComparer.Ordinal))
            throw OAuthException.FromUnauthorizedClient();

        var requestedScopes = ValueSet.FromDataString(request.Scope, ' ').Values.ToImmutableHashSet();

        if (requestedScopes.IsEmpty)
            throw OAuthException.FromInvalidScope();

        if (!requestedScopes.IsSubsetOf(client.AllowedScopes))
            throw OAuthException.FromInvalidScope();

        var context = new TokenRequestContext
        {
            Client = client,
            ClientAuthenticationMethod = method,
            GrantType = GrantType,
            Scopes = requestedScopes,
            TokenRequest = request,
        };

        return Task.FromResult(context);
    }
}
