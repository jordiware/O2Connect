using Microsoft.Extensions.Caching.Memory;

namespace O2Connect.Api.Repositories.Cache;

public interface IReplayCache
{
    Task<bool> ExistsAsync(string jti, CancellationToken ct = default);
    Task StoreAsync(string jti, DateTimeOffset expiresAt, CancellationToken ct = default);
    Task<bool> TryAddAsync(string jti, DateTimeOffset expiresAt, CancellationToken ct = default);
}

public class MemoryReplayCache : IReplayCache
{
    private readonly IMemoryCache _cache;

    public MemoryReplayCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<bool> ExistsAsync(string jti, CancellationToken ct = default)
    {
        return Task.FromResult(_cache.TryGetValue(jti, out _));
    }

    public Task StoreAsync(string jti, DateTimeOffset expiresAt, CancellationToken ct = default)
    {
        var ttl = expiresAt - DateTimeOffset.UtcNow;

        if (ttl <= TimeSpan.Zero)
            return Task.CompletedTask;

        _cache.Set(jti, true, ttl);

        return Task.CompletedTask;
    }

    public Task<bool> TryAddAsync(string jti, DateTimeOffset expiresAt, CancellationToken ct = default)
    {
        var ttl = expiresAt - DateTimeOffset.UtcNow;

        if (ttl <= TimeSpan.Zero)
            return Task.FromResult(false);

        if (_cache.TryGetValue(jti, out _))
            return Task.FromResult(false);

        _cache.Set(jti, true, ttl);

        return Task.FromResult(true);
    }
}
