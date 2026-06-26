using O2Connect.Api.Config;
using O2Connect.Api.Crypto;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.OidcOAuth.Connect;
using System.Collections.Immutable;

namespace O2Connect.Api.DataValidators.TokenRequestValidators;

public class RefreshTokenTokenRequestValidator : ITokenRequestValidator
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ISecureTokenGenerator _secureTokenGenerator;
    private readonly IJwtConfig _jwtConfig;

    public GrantType GrantType => GrantType.RefreshToken;

    public RefreshTokenTokenRequestValidator(
        IRefreshTokenRepository store,
        ISecureTokenGenerator secureTokenGenerator,
        IJwtConfig jwtConfig)
    {
        _refreshTokenRepository = store;
        _secureTokenGenerator = secureTokenGenerator;
        _jwtConfig = jwtConfig;
    }

    public async Task<TokenRequestContext> ValidateAsync(TokenRequest request,
                                                         Client client,
                                                         ClientAuthenticationMethod method,
                                                         CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw OAuthException.FromInvalidRequest();

        if (!client.AllowedGrantTypes.Contains(GrantType.Value, StringComparer.Ordinal))
            throw OAuthException.FromUnauthorizedClient();

        var token = await _refreshTokenRepository.GetAsync(request.RefreshToken, ct);

        if (token == null)
            throw OAuthException.FromInvalidGrant();

        if (token.ClientId != client.Id)
            throw OAuthException.FromInvalidGrant();

        if (token.IsRevoked)
            throw OAuthException.FromInvalidGrant();

        if (token.ExpiresAt < DateTimeOffset.UtcNow)
            throw OAuthException.FromInvalidGrant();

        if (token.IsConsumed)
        {
            if (!string.IsNullOrWhiteSpace(token.SessionId))
                await _refreshTokenRepository.RevokeSessionAsync(token.SessionId, ct);

            throw OAuthException.FromInvalidGrant();
        }

        var scopes = token.Scopes;
        var requestedScopes = ValueSet.FromDataString(request.Scope, ' ');

        if (!requestedScopes.IsEmpty && requestedScopes.IsSubsetOf(scopes))
            scopes = requestedScopes.Values.ToArray();

        var newToken = new RefreshToken
        {
            Token = _secureTokenGenerator.GenerateSecureToken(),
            ClientId = token.ClientId,
            Subject = token.Subject,
            Scopes = scopes,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtConfig.RefreshTokenLifetimeDays),
            SessionId = token.SessionId,
            Version = token.Version + 1,
        };

        await _refreshTokenRepository.ConsumeAndCreateAsync(token, newToken, ct);

        var context = new TokenRequestContext
        {
            Client = client,
            ClientAuthenticationMethod = method,
            GrantType = GrantType,
            Scopes = newToken.Scopes.ToHashSet(),
            TokenRequest = request,
            RefreshToken = newToken
        };

        return context;
    }
}
