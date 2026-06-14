using O2Connect.Api.Models.Store;
using O2Connect.Dto.Management.Clients;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories;

public interface IClientRepository
{
    Task<int> CountAsync(CancellationToken ct);
    Task<Client?> GetAsync(string clientId, CancellationToken ct);
    Task<IReadOnlyList<Client>> ListAsync(ClientListQuery listQuery, CancellationToken ct);
    Task StoreAsync(Client client, CancellationToken ct);
    Task<bool> ValidateClientAsync(string clientId, string? clientSecret, CancellationToken ct);
    Task<bool> ValidateRedirectUriAsync(string clientId, string redirectUri, CancellationToken ct);
}

public sealed record ClientListQuery(
    int Page,
    int PageSize,
    string SortBy,
    string Order
);

public class InMemoryClientRepository : IClientRepository
{
    private static readonly ConcurrentDictionary<string, Client> _clients = new();

    public Task<int> CountAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_clients.Count);
    }

    public Task<IReadOnlyList<Client>> ListAsync(ClientListQuery listQuery, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var orderAscending = listQuery.Order.Equals("asc", StringComparison.OrdinalIgnoreCase);

        var clients = listQuery.SortBy switch
        {
            _ => orderAscending 
                 ? _clients.OrderBy(c => c.Value.CreatedAt) 
                 : _clients.OrderByDescending(c => c.Value.CreatedAt)
        };

        var page = clients.Skip((listQuery.Page - 1) * listQuery.PageSize)
                          .Take(listQuery.PageSize)
                          .Select(kvp => kvp.Value)
                          .ToList();

        return Task.FromResult<IReadOnlyList<Client>>(page);
    }

    public Task<Client?> GetAsync(string clientId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _clients.TryGetValue(clientId, out var client);

        return Task.FromResult(client);
    }

    public Task StoreAsync(Client client, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _clients[client.ClientId] = client;
        
        return Task.CompletedTask;
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
