using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(string clientId, CancellationToken ct);
    Task<bool> ValidateClientAsync(string clientId, string? clientSecret, CancellationToken ct);
    Task<bool> ValidateRedirectUriAsync(string clientId, string redirectUri, CancellationToken ct);
}

public class InMemoryClientRepository : IClientRepository
{
    private static readonly ConcurrentDictionary<string, Client> _clients = new();

    public Task<Client?> GetByIdAsync(string clientId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _clients.TryGetValue(clientId, out var client);

        return Task.FromResult(client);
    }

    public Task<bool> ValidateClientAsync(string clientId, string? clientSecret, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _clients.TryGetValue(clientId, out var client);

        if (client == null)
            return Task.FromResult(false);

        if (string.IsNullOrEmpty(client.ClientSecret))
            return Task.FromResult(true);

        return Task.FromResult(client.ClientSecret == clientSecret);
    }

    public Task<bool> ValidateRedirectUriAsync(string clientId, string redirectUri, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _clients.TryGetValue(clientId, out var client);

        if (client == null)
            return Task.FromResult(false);

        return Task.FromResult(client.RedirectUris.Contains(redirectUri, StringComparer.Ordinal));
    }
}
