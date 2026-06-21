using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Repositories;

public interface IUserConsentRepository
{
    Task<UserConsent?> GetAsync(string userId, string clientId, CancellationToken ct);
    Task<IReadOnlyList<UserConsent>> GetForClientAsync(string clientId, CancellationToken ct);
    Task<IReadOnlyList<UserConsent>> GetForUserAsync(string userId, CancellationToken ct);
    Task StoreAsync(UserConsent consent, CancellationToken ct);
    Task RevokeAsync(string userId, string clientId, CancellationToken ct);
    Task RevokeForClientAsync(string clientId, CancellationToken ct);
    Task RevokeForUserAsync(string userId, CancellationToken ct);
}
