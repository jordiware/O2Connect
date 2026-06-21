using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories.InMemoryRepositories;

public class MemoryDeviceAuthorizationRepository : IDeviceAuthorizationRepository
{
    private static readonly ConcurrentDictionary<string, DeviceAuthorization> _authorizations = new();

    public Task<DeviceAuthorization?> GetAsync(string deviceCodeHash, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _authorizations.TryGetValue(deviceCodeHash, out var result);

        return Task.FromResult(result);
    }

    public Task<DeviceAuthorization?> GetByUserCodeAsync(string userCodeHash, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var result = _authorizations.Values.SingleOrDefault(da => string.Equals(da.UserCodeHash, userCodeHash, StringComparison.Ordinal));

        return Task.FromResult(result);
    }

    public Task StoreAsync(DeviceAuthorization authorization, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _authorizations[authorization.DeviceCodeHash] = authorization;

        return Task.CompletedTask;
    }
}
