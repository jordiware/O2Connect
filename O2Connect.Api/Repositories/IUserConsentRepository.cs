using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Repositories;

public interface IUserConsentRepository
{
    Task<UserConsent?> GetAsync(string userId, string clientId, CancellationToken ct);
    Task<bool> StoreAsync(UserConsent consent, CancellationToken ct);
    Task<bool> DeleteAsync(string userId, string clientId, CancellationToken ct);
}
