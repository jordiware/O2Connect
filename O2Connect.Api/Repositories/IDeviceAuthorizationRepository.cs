using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Repositories;

public interface IDeviceAuthorizationRepository
{
    Task<DeviceAuthorization?> GetAsync(string code, CancellationToken ct);
    Task<DeviceAuthorization?> GetByUserCodeAsync(string code, CancellationToken ct);
    Task StoreAsync(DeviceAuthorization authorization, CancellationToken ct);
}
