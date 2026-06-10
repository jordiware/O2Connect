using O2Connect.Api.DataFactories;
using O2Connect.Api.DataFactories.RequestModels;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Context;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.DataHandlers.TokenGrantHandlers;

public class ClientCredentialsGrantHandler : ITokenGrantHandler
{
    private readonly ITokenFactory _tokenFactory;

    public GrantType GrantType => GrantType.ClientCredentials;

    public ClientCredentialsGrantHandler(
        ITokenFactory tokenFactory)
    {
        _tokenFactory = tokenFactory;
    }

    public async Task<TokenResponse> HandleAsync(TokenRequestContext context, CancellationToken ct)
    {
        if (context.AuthorizationCode is not null)
            throw OAuthException.FromInvalidRequest();

        if (!context.Client.AllowedGrantTypes.Contains(GrantType.Value))
            throw OAuthException.FromUnauthorizedClient();

        var requestedScopes = context.Scopes;
        var allowedScopes = context.Client.AllowedScopes.ToHashSet();

        if (!requestedScopes.All(allowedScopes.Contains))
            throw OAuthException.FromInvalidScope();

        return await _tokenFactory.GenerateAsync(new JwtTokenFactoryRequest
        {
            ClientId = context.Client.ClientId,
            Scopes = context.Scopes,
            Subject = context.Client.ClientId
        }, ct);
    }
}
