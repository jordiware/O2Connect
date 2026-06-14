using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories.InMemoryRepositories;

public class MemoryAuthorizationCodeRepository : IAuthorizationCodeRepository
{
    private readonly ConcurrentDictionary<string, AuthorizationCode> _codes = new();

    public Task StoreAsync(AuthorizationCode code, CancellationToken ct)
    {
        _codes[code.Code] = code;
        return Task.CompletedTask;
    }

    public Task<AuthorizationCode?> GetAsync(string code, CancellationToken ct)
    {
        _codes.TryGetValue(code, out var value);
        return Task.FromResult(value);
    }

    public Task<AuthorizationCode?> RedeemAsync(string code, CancellationToken ct)
    {
        _codes.TryRemove(code, out var value);
        return Task.FromResult(value);
    }

    public Task<bool> TryConsumeAsync(string code, CancellationToken ct)
    {
        var updated = _codes.AddOrUpdate(code, _ => default!, (_, existing) =>
        {
            if (existing.Consumed)
                return existing;

            return existing with { Consumed = true };
        });

        return Task.FromResult(updated.Consumed);
    }

    public Task RemoveAsync(string code, CancellationToken ct)
    {
        _codes.Remove(code, out _);
        return Task.CompletedTask;
    }
}
