using O2Connect.Api.Models;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories;

public interface IAuthorizationCodeRepository
{
    Task StoreAsync(AuthorizationCode code, CancellationToken ct);
    Task<AuthorizationCode?> GetAsync(string code, CancellationToken ct);
    Task<AuthorizationCode?> RedeemAsync(string code, CancellationToken ct);
    Task RemoveAsync(string code, CancellationToken ct);
}

public class InMemoryAuthorizationCodeRepository : IAuthorizationCodeRepository
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

    public Task RemoveAsync(string code, CancellationToken ct)
    {
        _codes.Remove(code, out _);
        return Task.CompletedTask;
    }
}
