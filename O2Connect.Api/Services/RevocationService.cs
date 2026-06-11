using O2Connect.Api.Exceptions;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Services;

public interface IRevocationService
{
    Task HandleAsync(RevocationRequest request, string? clientId, CancellationToken ct);
}

public class RevocationService : IRevocationService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RevocationService(
        IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task HandleAsync(RevocationRequest request, string? clientId, CancellationToken ct)
    {
        var token = request.Token?.Trim();

        if (string.IsNullOrWhiteSpace(token))
            throw OAuthException.FromInvalidRequest("token is required");

        if (string.IsNullOrWhiteSpace(clientId))
            throw OAuthException.FromInvalidClient();

        if (token.Length > 512)
            return;

        if (!string.IsNullOrEmpty(request.TokenTypeHint) 
            && !string.Equals(request.TokenTypeHint, "refresh_token", StringComparison.Ordinal))
            return;

        ct.ThrowIfCancellationRequested();

        var refreshToken = await _refreshTokenRepository.GetAsync(token, ct);

        if (refreshToken is { ClientId: var tokenClientId } 
            && string.Equals(tokenClientId, clientId, StringComparison.Ordinal))
            await _refreshTokenRepository.RevokeSessionAsync(refreshToken.SessionId, ct);
    }
}
