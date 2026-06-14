using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories.InMemoryRepositories;

public class MemoryRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ConcurrentDictionary<string, RefreshToken> _tokens = new();

    public Task CreateAsync(RefreshToken token, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_tokens.TryAdd(token.Token, token))
            throw new InvalidOperationException("Refresh token already exists.");

        return Task.CompletedTask;
    }

    public Task<RefreshToken?> GetAsync(string token, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _tokens.TryGetValue(token, out var value);
        return Task.FromResult(value);
    }

    public Task<bool> IsConsumedAsync(string token, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (_tokens.TryGetValue(token, out var value))
            return Task.FromResult(value.Consumed);

        return Task.FromResult(false);
    }

    public Task<bool> TryConsumeAsync(string token, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var updated = _tokens.AddOrUpdate(token, _ => default!, (_, existing) =>
        {
            return existing with
            {
                Consumed = true,
                ConsumedAt = DateTimeOffset.UtcNow
            };
        });

        return Task.FromResult(updated.Consumed);
    }

    public Task RevokeAsync(string token, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (_tokens.TryGetValue(token, out var existing))
        {
            _tokens[token] = existing with
            {
                Revoked = true,
                RevokedAt = DateTimeOffset.UtcNow
            };
        }

        return Task.CompletedTask;
    }

    public Task RevokeSessionAsync(string sessionId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var kvp = _tokens.SingleOrDefault(kvp => kvp.Value.SessionId.Equals(sessionId, StringComparison.Ordinal));

        if (!_tokens.ContainsKey(kvp.Key))
            return Task.CompletedTask;

        _tokens[kvp.Key] = kvp.Value with
        {
            Revoked = true,
            RevokedAt = DateTimeOffset.UtcNow
        };

        return Task.CompletedTask;
    }

    public Task RevokeSubjectAsync(string subjectId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var kvp = _tokens.SingleOrDefault(kvp => kvp.Value.Subject.Equals(subjectId, StringComparison.Ordinal));

        if (!_tokens.ContainsKey(kvp.Key))
            return Task.CompletedTask;

        _tokens[kvp.Key] = kvp.Value with
        {
            Revoked = true,
            RevokedAt = DateTimeOffset.UtcNow
        };

        return Task.CompletedTask;
    }

    public Task<RefreshToken?> RotateAsync(string token, RefreshToken newToken, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_tokens.TryGetValue(token, out var existing))
            return Task.FromResult<RefreshToken?>(null);

        if (existing.Consumed)
            return Task.FromResult<RefreshToken?>(null);

        // mark old token as consumed
        existing.Consumed = true;
        existing.ConsumedAt = DateTimeOffset.UtcNow;

        // link chain (optional but useful)
        existing.ReplacedByToken = newToken.Token;

        // store new token
        _tokens[newToken.Token] = newToken;

        return Task.FromResult<RefreshToken?>(newToken);
    }

    public Task ConsumeAndCreateAsync(RefreshToken token, RefreshToken newToken, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (_tokens.TryGetValue(token.Token, out var existing))
        {
            if (existing.Consumed)
                throw new InvalidOperationException("Refresh token already consumed.");

            existing.Consumed = true;
            existing.ConsumedAt = DateTimeOffset.UtcNow;
            existing.ReplacedByToken = newToken.Token;
        }

        _tokens[newToken.Token] = newToken;

        return Task.CompletedTask;
    }

    public Task<bool> IsSessionActiveAsync(string sessionId, CancellationToken ct)
    {
        var kvp = _tokens.Where(kvp => kvp.Value.SessionId.Equals(sessionId, StringComparison.Ordinal)
                                       && !kvp.Value.Consumed
                                       && !kvp.Value.Revoked
                                       && kvp.Value.ExpiresAt > DateTimeOffset.UtcNow);

        return Task.FromResult(kvp.Any());
    }
}
