using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Context;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using System.Security.Cryptography;

namespace O2Connect.Api.Controllers.RequestModelValidators;

public class RefreshTokenTokenRequestValidator : ITokenRequestValidator
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public GrantType GrantType => GrantType.RefreshToken;

    public RefreshTokenTokenRequestValidator(IRefreshTokenRepository store)
    {
        _refreshTokenRepository = store;
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

        if (token.ClientId != client.ClientId)
            throw OAuthException.FromInvalidGrant();

        if (token.Revoked)
            throw OAuthException.FromInvalidGrant();

        if (token.ExpiresAt < DateTimeOffset.UtcNow)
            throw OAuthException.FromInvalidGrant();

        if (token.Consumed)
        {
            if (!string.IsNullOrWhiteSpace(token.SessionId))
                await _refreshTokenRepository.RevokeSessionAsync(token.SessionId, ct);

            throw OAuthException.FromInvalidGrant();
        }

        var scopes = token.Scopes;
        var requestedScopes = ValueSet.FromDataString(request.Scope, ' ');
        if (!requestedScopes.IsEmpty && requestedScopes.IsSubsetOf(token.Scopes.Values))
            scopes = requestedScopes;

        var newToken = new RefreshToken
        {
            Token = GenerateSecureToken(),
            ClientId = token.ClientId,
            Subject = token.Subject,
            Scopes = scopes,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
            SessionId = token.SessionId,
            Version = token.Version + 1,
        };

        await _refreshTokenRepository.ConsumeAndCreateAsync(token, newToken, ct);

        var context = new TokenRequestContext
        {
            Client = client,
            ClientAuthenticationMethod = method,
            GrantType = GrantType,
            Scopes = newToken.Scopes,
            TokenRequest = request,
        };

        return context;
    }

    private static string GenerateSecureToken(int numBytes = 64)
    {
        Span<byte> bytes = numBytes <= 256
            ? stackalloc byte[numBytes]
            : new byte[numBytes];

        RandomNumberGenerator.Fill(bytes);

        var token = Convert.ToBase64String(bytes)
                           .Replace("+", "-")
                           .Replace("/", "_")
                           .TrimEnd('=');

        return $"rt_{token}";
    }
}
