using O2Connect.Api.DataFactories;
using O2Connect.Api.DataFactories.RequestModels;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Context;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.DataHandlers.TokenGrantHandlers;

public class AuthorizationCodeGrantHandler : ITokenGrantHandler
{
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly ITokenFactory _tokenFactory;

    public GrantType GrantType => GrantType.AuthorizationCode;

    public AuthorizationCodeGrantHandler(
        IAuthorizationCodeRepository authorizationCodeRepository,
        ITokenFactory tokenFactory)
    {
        _authorizationCodeRepository = authorizationCodeRepository;
        _tokenFactory = tokenFactory;
    }

    public async Task<TokenResponse> HandleAsync(TokenRequestContext context, CancellationToken ct)
    {
        var authCode = context.AuthorizationCode ?? throw OAuthException.FromInvalidRequest();

        var storedCode = await _authorizationCodeRepository.RedeemAsync(authCode.Code, ct)
            ?? throw OAuthException.FromInvalidGrant();

        if (storedCode.ClientId != context.Client.ClientId)
            throw OAuthException.FromInvalidGrant();

        if (storedCode.ExpiresAt <= DateTime.UtcNow)
            throw OAuthException.FromInvalidGrant();

        if (!Uri.TryCreate(authCode.RedirectUri, UriKind.Absolute, out var contextRedirectUri) 
            || !Uri.TryCreate(storedCode.RedirectUri, UriKind.Absolute, out var storedRedirectUri))
            throw OAuthException.FromInvalidGrant();

        if (!contextRedirectUri.Equals(storedRedirectUri))
            throw OAuthException.FromInvalidGrant();

        var contextScopes = context.Scopes;
        var grantedScopes = storedCode.Scopes;

        if (!contextScopes.All(grantedScopes.Contains))
            throw OAuthException.FromInvalidScope();

        return await _tokenFactory.GenerateAsync(new JwtTokenFactoryRequest
        {
            Client = context.Client,
            Scopes = context.Scopes,
            Subject = storedCode.SubjectId
        }, ct);
    }
}
