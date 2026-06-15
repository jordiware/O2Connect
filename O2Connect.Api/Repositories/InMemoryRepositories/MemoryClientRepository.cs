using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories.Filters;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories.InMemoryRepositories;

public class MemoryClientRepository : IClientRepository
{
    private static readonly ConcurrentDictionary<string, Client> _clients = new();

    public Task<int> CountAsync(CancellationToken ct)
    {
        return CountAsync(ClientFilter.Empty, ct);
    }

    public Task<int> CountAsync(ClientFilter filter, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_clients.Values.Count(filter.Filter));
    }

    public Task<IReadOnlyList<Client>> QueryAsync(ClientQuery listQuery, CancellationToken ct)
    {
        return QueryAsync(listQuery, ClientFilter.Empty, ct);
    }

    public Task<IReadOnlyList<Client>> QueryAsync(ClientQuery listQuery,
                                                 ClientFilter filter,
                                                 CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var orderAscending = listQuery.Order.Equals("asc", StringComparison.OrdinalIgnoreCase);

        var filtered = _clients.Values.Where(filter.Filter);

        var clients = listQuery.SortBy switch
        {
            _ => orderAscending
                 ? filtered.OrderBy(c => c.NormalizedName,
                                         StringComparer.InvariantCultureIgnoreCase)
                 : filtered.OrderByDescending(c => c.NormalizedName,
                                                   StringComparer.InvariantCultureIgnoreCase)
        };

        var page = clients.Skip((listQuery.Page - 1) * listQuery.PageSize)
                          .Take(listQuery.PageSize)
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

        _clients[client.Id] = client;

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
