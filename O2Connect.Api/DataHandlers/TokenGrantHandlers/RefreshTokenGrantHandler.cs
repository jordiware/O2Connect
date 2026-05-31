using O2Connect.Api.DataFactories;
using O2Connect.Api.DataFactories.RequestModels;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Context;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Responses;
using System.Security.Cryptography;

namespace O2Connect.Api.DataHandlers.TokenGrantHandlers;

public class RefreshTokenGrantHandler : ITokenGrantHandler
{
    private readonly ITokenFactory _tokenFactory;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public GrantType GrantType => GrantType.RefreshToken;

    public RefreshTokenGrantHandler(
        ITokenFactory tokenFactory,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _tokenFactory = tokenFactory;
        _refreshTokenRepository = refreshTokenRepository;
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

        if (storedToken.ClientId != context.Client.ClientId)
            throw OAuthException.FromInvalidGrant();

        var originalScopes = storedToken.Scopes.Values.ToHashSet();
        var requestedScopes = context.Scopes.Values.ToHashSet();

        if (!requestedScopes.All(originalScopes.Contains))
            throw OAuthException.FromInvalidScope();

        var newRefreshToken = new RefreshToken
        {
            Token = GenerateSecureToken(),
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
            Client = context.Client,
            Subject = storedToken.Subject,
            Scopes = context.Scopes,
            RefreshToken = newRefreshToken.Token
        }, ct);
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
