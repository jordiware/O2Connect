using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories.InMemoryRepositories;

public class MemoryRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ConcurrentDictionary<string, RefreshToken> _tokens = new();

    public Task ConsumeAndCreateAsync(RefreshToken? token, RefreshToken newToken, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (token is not null && _tokens.TryGetValue(token.Token, out var existing))
        {
            if (existing.IsConsumed)
                throw new InvalidOperationException("Refresh token already consumed.");

            existing = existing with
            {
                ConsumedAt = DateTimeOffset.UtcNow,
                ReplacedByToken = newToken.Token
            };

            _tokens[existing.Token] = existing;
        }

        _tokens[newToken.Token] = newToken;

        return Task.CompletedTask;
    }

    public Task<RefreshToken?> GetAsync(string token, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _tokens.TryGetValue(token, out var value);
        return Task.FromResult(value);
    }

    public Task<bool> IsSessionActiveAsync(string sessionId, CancellationToken ct)
    {
        var kvp = _tokens.Where(kvp => kvp.Value.SessionId.Equals(sessionId, StringComparison.Ordinal)
                                       && !kvp.Value.IsConsumed
                                       && !kvp.Value.IsRevoked
                                       && kvp.Value.ExpiresAt > DateTimeOffset.UtcNow);

        return Task.FromResult(kvp.Any());
    }

    public Task RevokeClientAsync(string clientId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var utcNow = DateTimeOffset.UtcNow;

        var tokens = _tokens.Where(kvp => kvp.Value.ClientId.Equals(clientId, StringComparison.Ordinal));

        foreach (var token in tokens)
        {
            _tokens[token.Key] = token.Value with
            {
                RevokedAt = utcNow
            };
        }

        return Task.CompletedTask;
    }

    public Task RevokeSessionAsync(string sessionId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var utcNow = DateTimeOffset.UtcNow;

        var tokens = _tokens.Where(kvp => kvp.Value.SessionId.Equals(sessionId, StringComparison.Ordinal));

        foreach (var token in tokens)
        {
            _tokens[token.Key] = token.Value with
            {
                RevokedAt = utcNow
            };
        }

        return Task.CompletedTask;
    }

    public Task RevokeSubjectAsync(string subjectId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var utcNow = DateTimeOffset.UtcNow;

        var tokens = _tokens.Where(kvp => kvp.Value.Subject.Equals(subjectId, StringComparison.Ordinal));

        foreach (var token in tokens)
        {
            _tokens[token.Key] = token.Value with
            {
                RevokedAt = utcNow
            };
        }

        return Task.CompletedTask;
    }

    public Task RevokeForSubjectAndClientAsync(string subjectId, string clientId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var utcNow = DateTimeOffset.UtcNow;

        var tokens = _tokens.Where(kvp => kvp.Value.Subject.Equals(subjectId, StringComparison.Ordinal)
                                          && kvp.Value.ClientId.Equals(clientId, StringComparison.Ordinal));

        foreach (var token in tokens)
        {
            _tokens[token.Key] = token.Value with
            {
                RevokedAt = utcNow
            };
        }

        return Task.CompletedTask;
    }

    public Task<RefreshToken?> RotateAsync(string token, RefreshToken newToken, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_tokens.TryGetValue(token, out var existing))
            return Task.FromResult<RefreshToken?>(null);

        if (existing.IsConsumed)
            return Task.FromResult<RefreshToken?>(null);

        existing = existing with
        {
            ConsumedAt = DateTimeOffset.UtcNow,
            ReplacedByToken = newToken.Token
        };

        _tokens[existing.Token] = existing;
        _tokens[newToken.Token] = newToken;

        return Task.FromResult<RefreshToken?>(newToken);
    }
}
