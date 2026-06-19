using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories.Filters;

namespace O2Connect.Api.Repositories;

public interface IClientRepository
{
    Task<int> CountAsync(CancellationToken ct);
    Task<int> CountAsync(ClientFilter filter, CancellationToken ct);
    Task<Client?> GetAsync(string clientId, CancellationToken ct);
    Task<IReadOnlyList<Client>> QueryAsync(EntityPagination listQuery, CancellationToken ct);
    Task<IReadOnlyList<Client>> QueryAsync(EntityPagination listQuery,
                                           ClientFilter filter,
                                           CancellationToken ct);
    Task StoreAsync(Client client, CancellationToken ct);
    Task<bool> ValidateClientAsync(string clientId, string? clientSecret, CancellationToken ct);
    Task<bool> ValidateRedirectUriAsync(string clientId, string redirectUri, CancellationToken ct);
}
