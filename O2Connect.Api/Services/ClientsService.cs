using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Mappers;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Api.Repositories.Filters;
using O2Connect.Dto.Management.Clients;

namespace O2Connect.Api.Services;

public interface IClientsService
{
    Task<ClientDetailsResponse?> GetClientAsync(string clientId,
                                                CancellationToken ct);
    Task<ClientListResponse> QueryClientsAsync(ClientPagination pagination,
                                               ClientFilter filter,
                                               CancellationToken ct);
    Task UpdateClientRedirectUrisAsync(string clientId,
                                       IReadOnlyList<string> redirectUris,
                                       CancellationToken ct);
    Task UpdateClientScopesAsync(string clientId,
                                 IReadOnlyList<string> scopes,
                                 CancellationToken ct);
    Task UpdateClientStatusAsync(string clientId,
                                 string status,
                                 CancellationToken ct);
}

public class ClientsService : IClientsService
{
    private readonly IClientRepository _repository;
    private readonly ILogger<ClientsService> _logger;

    public ClientsService(
        IClientRepository repository,
        ILogger<ClientsService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ClientDetailsResponse?> GetClientAsync(string clientId, CancellationToken ct)
    {
        var client = await _repository.GetAsync(clientId, ct);

        if (client is null)
        {
            _logger.LogWarning("Client with ID {ClientId} not found.", clientId);
            return null;
        }

        var response = client.ToDetails();

        return response;
    }

    public async Task<ClientListResponse> QueryClientsAsync(ClientPagination pagination,
                                                            ClientFilter filter,
                                                            CancellationToken ct)
    {
        var totalClients = await _repository.CountAsync(filter, ct);

        if (totalClients == 0)
        {
            return new ClientListResponse
            {
                Items = [],
                TotalItems = 0,
                Page = pagination.Page,
                TotalPages = 0,
                PageSize = pagination.PageSize
            };
        }

        var pages = (int)Math.Ceiling((double)totalClients / pagination.PageSize);

        if (pagination.Page > pages)
        {
            _logger.LogWarning("Requested page {Page} is out of range. Total pages: {Pages}", pagination.Page, pages);
            throw ApiException.BadRequest("invalid_requested_page", $"Page must be between 1 and {pages}");
        }

        var skip = (pagination.Page - 1) * pagination.PageSize;
        var remainingItems = totalClients - skip;
        var pageSize = Math.Min(pagination.PageSize, remainingItems);

        pagination = pagination with { PageSize = pageSize };

        var clients = await _repository.QueryAsync(pagination, filter, ct);

        var summaries = clients.Select(c => c.ToSummary()).ToList();

        var response = new ClientListResponse
        {
            Items = summaries,
            TotalItems = totalClients,
            Page = pagination.Page,
            TotalPages = pages,
            PageSize = pagination.PageSize
        };

        return response;
    }

    public async Task UpdateClientRedirectUrisAsync(string clientId,
                                                    IReadOnlyList<string> redirectUris,
                                                    CancellationToken ct)
    {
        var newRedirectUris = redirectUris.Select(r => r.Trim())
                                          .Where(r => !string.IsNullOrWhiteSpace(r))
                                          .Distinct()
                                          .ToHashSet();

        if (!newRedirectUris.All(r => Uri.IsWellFormedUriString(r, UriKind.Absolute)))
        {
            _logger.LogWarning("Invalid redirect URIs provided for client {ClientId}.", clientId);
            throw ApiException.BadRequest("invalid_redirect_uris", "One or more redirect URIs are invalid.");
        }

        var client = await GetClientForUpdateAsync(clientId, ct);

        var now = DateTimeOffset.UtcNow;

        client = client with
        {
            RedirectUris = newRedirectUris,
            LastModifiedAt = now
        };

        await _repository.StoreAsync(client, ct);
    }

    public async Task UpdateClientScopesAsync(string clientId,
                                              IReadOnlyList<string> scopes,
                                              CancellationToken ct)
    {
        var newScopes = scopes.Select(s => s.Trim())
                              .Where(s => !string.IsNullOrWhiteSpace(s))
                              .Distinct()
                              .ToHashSet();

        if (!newScopes.All(Scopes.All.Contains))
        {
            var invalidScopes = newScopes.Except(Scopes.All);

            _logger.LogWarning("Invalid scopes provided: {InvalidScopes}", string.Join(", ", invalidScopes));

            throw ApiException.BadRequest("invalid_requested_scopes",
                                          $"Invalid scopes provided: {string.Join(", ", invalidScopes)}");
        }

        var client = await GetClientForUpdateAsync(clientId, ct);

        var now = DateTimeOffset.UtcNow;

        client = client with
        {
            AllowedScopes = newScopes,
            LastModifiedAt = now
        };

        await _repository.StoreAsync(client, ct);
    }

    public async Task UpdateClientStatusAsync(string clientId, string status, CancellationToken ct)
    {
        status = status.Trim();

        if (!Enum.TryParse<EntityStatus>(status, true, out var newStatus))
        {
            _logger.LogWarning("Invalid status value: {Status}", status);
            throw ApiException.BadRequest("invalid_requested_status", $"Invalid status value: {status}");
        }

        var client = await GetClientForUpdateAsync(clientId, ct);

        if (client.Status == newStatus)
            return;

        var now = DateTimeOffset.UtcNow;

        client = client with
        {
            Status = newStatus,
            LastModifiedAt = now
        };

        if (newStatus == EntityStatus.Revoked)
            client = client with { RevokedAt = now };

        await _repository.StoreAsync(client, ct);
    }

    private async Task<Client> GetClientForUpdateAsync(string clientId, CancellationToken ct)
    {
        var client = await _repository.GetAsync(clientId, ct);

        if (client is null)
        {
            _logger.LogInformation("Client with ID '{ClientId}' not found.", clientId);
            throw ApiException.NotFound("client_not_found", $"Client '{clientId}' not found.");
        }

        if (client.Status == EntityStatus.Revoked)
        {
            _logger.LogInformation("Client '{ClientId}' is already revoked. No updates performed.", clientId);
            throw ApiException.Conflict("client_revoked",
                                        $"Client '{clientId}' is revoked and updates cannot be performed.");
        }

        return client;
    }
}
