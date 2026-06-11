using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories;

public interface IDeviceAuthorizationRepository
{
    Task<DeviceAuthorization?> GetAsync(string code, CancellationToken ct);
    Task StoreAsync(DeviceAuthorization authorization, CancellationToken ct);
}

public class InMemoryDeviceAuthorizationRepository : IDeviceAuthorizationRepository
{
    private static readonly ConcurrentDictionary<string, DeviceAuthorization> _authorizations = new();

    public Task<DeviceAuthorization?> GetAsync(string code, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _authorizations.TryGetValue(code, out var result);
        
        return Task.FromResult(result);
    }

    public Task StoreAsync(DeviceAuthorization authorization, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _authorizations[authorization.DeviceCodeHash] = authorization;

        return Task.CompletedTask;
    }
}
