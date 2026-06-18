using O2Connect.Api.Crypto;
using O2Connect.Api.DataFactories;
using O2Connect.Api.DataFactories.RequestModels;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.OidcOAuth.Connect;

namespace O2Connect.Api.DataHandlers.TokenContextHandlers;

public class RefreshTokenContextHandler : ITokenContextHandler
{
    private readonly ITokenFactory _tokenFactory;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ISecureTokenGenerator _secureTokenGenerator;

    public GrantType GrantType => GrantType.RefreshToken;

    public RefreshTokenContextHandler(
        ITokenFactory tokenFactory,
        IRefreshTokenRepository refreshTokenRepository,
        ISecureTokenGenerator secureTokenGenerator)
    {
        _tokenFactory = tokenFactory;
        _refreshTokenRepository = refreshTokenRepository;
        _secureTokenGenerator = secureTokenGenerator;
    }

    public async Task<TokenResponse> HandleAsync(TokenRequestContext context, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(context.RefreshToken?.Token ?? string.Empty))
            throw OAuthException.FromInvalidRequest();

        var contextToken = context.RefreshToken!.Token;

        if (!context.Client.AllowedGrantTypes.Contains(GrantType.Value))
            throw OAuthException.FromUnauthorizedClient();

        var storedToken = await _refreshTokenRepository.GetAsync(contextToken, ct);
        var now = DateTimeOffset.UtcNow;

        if (storedToken is null)
            throw OAuthException.FromInvalidGrant();

        if (storedToken.ExpiresAt <= now)
            throw OAuthException.FromInvalidGrant();

        if (storedToken.Consumed)
        {
            await _refreshTokenRepository.RevokeSessionAsync(storedToken.SessionId, ct);
            throw OAuthException.FromInvalidGrant();
        }

        if (storedToken.Revoked)
            throw OAuthException.FromInvalidGrant();

        if (storedToken.ClientId != context.Client.Id)
            throw OAuthException.FromInvalidGrant();

        var originalScopes = storedToken.Scopes;
        var requestedScopes = context.Scopes;

        if (!requestedScopes.All(originalScopes.Contains))
            throw OAuthException.FromInvalidScope();

        var newRefreshToken = new RefreshToken
        {
            Token = _secureTokenGenerator.GenerateSecureToken(),
            ClientId = storedToken.ClientId,
            Subject = storedToken.Subject,
            Scopes = context.Scopes,
            CreatedAt = now,
            ExpiresAt = now.AddDays(30),
            Consumed = false,
            Revoked = false,
            SessionId = storedToken.SessionId,
            Version = storedToken.Version + 1
        };

        await _refreshTokenRepository.RotateAsync(storedToken.Token, newRefreshToken, ct);

        return await _tokenFactory.GenerateAsync(new JwtTokenFactoryRequest
        {
            ClientId = context.Client.Id,
            Subject = storedToken.Subject,
            Scopes = context.Scopes,
            RefreshToken = newRefreshToken.Token
        }, ct);
    }
}
