using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;
using static O2Connect.Api.Models.Scopes;

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

    public Task<IReadOnlyList<UserConsent>> GetForClientAsync(string clientId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var keys = _consents.Keys.Where(k => k.ClientId.Equals(clientId, StringComparison.Ordinal));

        var consents = _consents.Where(kvp => keys.Contains(kvp.Key))
                                .Select(kvp => kvp.Value)
                                .ToList();

        return Task.FromResult<IReadOnlyList<UserConsent>>(consents);
    }

    public Task<IReadOnlyList<UserConsent>> GetForUserAsync(string userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var keys = _consents.Keys.Where(k => k.UserId.Equals(userId, StringComparison.Ordinal));

        var consents = _consents.Where(kvp => keys.Contains(kvp.Key))
                                .Select(kvp => kvp.Value)
                                .ToList();

        return Task.FromResult<IReadOnlyList<UserConsent>>(consents);
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

    public Task RevokeForClientAsync(string clientId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var keys = _consents.Keys.Where(k => k.ClientId.Equals(clientId, StringComparison.Ordinal));

        foreach (var key in keys)
        {
            _consents.Remove(key, out _);
        }

        return Task.CompletedTask;
    }

    public Task RevokeForUserAsync(string userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var keys = _consents.Keys.Where(k => k.UserId.Equals(userId, StringComparison.Ordinal));

        foreach (var key in keys)
        {
            _consents.Remove(key, out _);
        }

        return Task.CompletedTask;
    }

    private sealed record UserClientKey(string UserId, string ClientId);
}
