using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Repositories;

public interface IDeviceAuthorizationRepository
{
    Task<DeviceAuthorization?> GetAsync(string deviceCodeHash, CancellationToken ct);
    Task<DeviceAuthorization?> GetByUserCodeAsync(string userCodeHash, CancellationToken ct);
    Task StoreAsync(DeviceAuthorization authorization, CancellationToken ct);
}
