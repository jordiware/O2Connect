using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetAsync(string token, CancellationToken ct);
    Task CreateAsync(RefreshToken token, CancellationToken ct);
    Task ConsumeAndCreateAsync(RefreshToken token, RefreshToken newToken, CancellationToken ct);
    Task<bool> TryConsumeAsync(string token, CancellationToken ct);
    Task<bool> IsConsumedAsync(string token, CancellationToken ct);
    Task RevokeAsync(string token, CancellationToken ct);
    Task RevokeSessionAsync(string sessionId, CancellationToken ct);
    Task<RefreshToken?> RotateAsync(string token, RefreshToken newToken, CancellationToken ct);
}

public class InMemoryRefreshTokenRepository : IRefreshTokenRepository
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
            if (existing.Consumed)
                return existing;

            existing.Consumed = true;
            existing.ConsumedAt = DateTimeOffset.UtcNow;
            return existing;
        });

        return Task.FromResult(updated.Consumed);
    }

    public Task RevokeAsync(string token, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (_tokens.TryGetValue(token, out var existing))
        {
            existing.Revoked = true;
            existing.RevokedAt = DateTimeOffset.UtcNow;
        }

        return Task.CompletedTask;
    }

    public Task RevokeSessionAsync(string sessionId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        foreach (var kvp in _tokens)
        {
            var token = kvp.Value;

            if (token.SessionId == sessionId)
            {
                token.Revoked = true;
                token.RevokedAt = DateTimeOffset.UtcNow;
            }
        }

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
}
