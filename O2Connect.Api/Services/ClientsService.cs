using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Api.Repositories.Filters;
using O2Connect.Dto.Management.Clients;

namespace O2Connect.Api.Services;

public interface IClientsService
{
    Task<ClientListResponse> ListClientsAsync(ListClientsRequest request, CancellationToken ct);
    Task<ClientListResponse> SearchClientsAsync(ListClientsRequest listRequest,
                                                ClientSearchRequest searchRequest,
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

    public async Task<ClientListResponse> ListClientsAsync(ListClientsRequest request, CancellationToken ct)
    {
        var totalClients = await _repository.CountAsync(ct);

        if (totalClients == 0)
        {
            return new ClientListResponse
            {
                Items = [],
                TotalItems = 0,
                Page = request.Page,
                TotalPages = 0,
                PageSize = request.PageSize
            };
        }

        var pages = (int)Math.Ceiling((double)totalClients / request.PageSize);

        if (request.Page > pages)
        {
            _logger.LogWarning("Requested page {Page} is out of range. Total pages: {Pages}", request.Page, pages);
            throw OAuthException.FromInvalidRequest($"Page must be between 1 and {pages}");
        }

        var pageSize = totalClients - (request.Page * request.PageSize);
        pageSize = Math.Min(pageSize, request.PageSize);

        var listQuery = new ClientQuery(
            Page: request.Page,
            PageSize: pageSize,
            SortBy: request.SortBy ?? "ClientName",
            Order: request.Order ?? "asc"
        );

        var clients = await _repository.QueryAsync(listQuery, ct);

        var summaries = clients.Select(client => new ClientSummaryDto
        {
            Id = client.Id,
            Name = client.Name,
            ImageUrl = client.ImageUrl,
            Status = client.Status.ToString().ToLowerInvariant(),
        }).ToList();

        var response = new ClientListResponse
        {
            Items = summaries,
            TotalItems = totalClients,
            Page = request.Page,
            TotalPages = pages,
            PageSize = request.PageSize
        };

        return response;
    }

    public async Task<ClientListResponse> SearchClientsAsync(ListClientsRequest listRequest,
                                                             ClientSearchRequest searchRequest,
                                                             CancellationToken ct)
    {
        var statuses = searchRequest.Status?.Select(s => Enum.Parse<EntityStatus>(s, true)).ToHashSet();

        var filter = new ClientFilter
        {
            Name = searchRequest.Name,
            Status = statuses,
            MinCreatedAt = searchRequest.MinCreatedAt,
            MaxCreatedAt = searchRequest.MaxCreatedAt,
            MinLastModifiedAt = searchRequest.MinLastModifiedAt,
            MaxLastModifiedAt = searchRequest.MaxLastModifiedAt,
            MinRevokedAt = searchRequest.MinRevokedAt,
            MaxRevokedAt = searchRequest.MaxRevokedAt,
            GrantTypes = searchRequest.GrantTypes?.ToHashSet(StringComparer.Ordinal),
            Scopes = searchRequest.Scopes?.ToHashSet(StringComparer.Ordinal),
            AuthenticationMethods = searchRequest.AuthenticationMethods?.ToHashSet(StringComparer.Ordinal),
            ResponseTypes = searchRequest.ResponseTypes?.ToHashSet(StringComparer.Ordinal)
        };

        var totalClients = await _repository.CountAsync(filter, ct);

        if (totalClients == 0)
        {
            return new ClientListResponse
            {
                Items = [],
                TotalItems = 0,
                Page = listRequest.Page,
                TotalPages = 0,
                PageSize = listRequest.PageSize
            };
        }

        var pages = (int)Math.Ceiling((double)totalClients / listRequest.PageSize);

        if (listRequest.Page > pages)
        {
            _logger.LogWarning("Requested page {Page} is out of range. Total pages: {Pages}", listRequest.Page, pages);
            throw OAuthException.FromInvalidRequest($"Page must be between 1 and {pages}");
        }

        var pageSize = totalClients - (listRequest.Page * listRequest.PageSize);
        pageSize = Math.Min(pageSize, listRequest.PageSize);

        var listQuery = new ClientQuery(
            Page: listRequest.Page,
            PageSize: pageSize,
            SortBy: listRequest.SortBy ?? "ClientName",
            Order: listRequest.Order ?? "asc"
        );

        var clients = await _repository.QueryAsync(listQuery, filter, ct);

        var summaries = clients.Select(client => new ClientSummaryDto
        {
            Id = client.Id,
            Name = client.Name,
            ImageUrl = client.ImageUrl,
            Status = client.Status.ToString().ToLowerInvariant(),
        }).ToList();

        var response = new ClientListResponse
        {
            Items = summaries,
            TotalItems = totalClients,
            Page = listRequest.Page,
            TotalPages = pages,
            PageSize = listRequest.PageSize
        };

        return response;
    }
}
