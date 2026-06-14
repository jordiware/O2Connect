using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories.InMemoryRepositories;

public class MemoryUserConsentRepository : IUserConsentRepository
{
    private readonly ConcurrentDictionary<UserClientKey, UserConsent> _consents = new();

    public Task<bool> DeleteAsync(string userId, string clientId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var key = new UserClientKey(userId, clientId);
        var removed = _consents.TryRemove(key, out var value);
        return Task.FromResult(removed);
    }

    public Task<UserConsent?> GetAsync(string userId, string clientId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var key = new UserClientKey(userId, clientId);
        _consents.TryGetValue(key, out var value);
        return Task.FromResult(value);
    }

    public Task<bool> StoreAsync(UserConsent consent, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var key = new UserClientKey(consent.UserId, consent.ClientId);
        var newStoredConsent = _consents.AddOrUpdate(key, 
                                                     key => consent, 
                                                     (key, oldConsent) => oldConsent = consent);
        return Task.FromResult(consent == newStoredConsent);
    }

    private sealed record UserClientKey(string UserId, string ClientId);
}
