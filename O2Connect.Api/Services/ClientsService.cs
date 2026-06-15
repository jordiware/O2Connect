using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Mappers;
using O2Connect.Api.Repositories;
using O2Connect.Api.Repositories.Filters;
using O2Connect.Dto.Management.Clients;

namespace O2Connect.Api.Services;

public interface IClientsService
{
    Task<ClientDetailsResponse?> GetClientAsync(string clientId, CancellationToken ct);
    Task<ClientListResponse> QueryClientsAsync(ClientsPaginationRequest listRequest, ClientFilter filter, CancellationToken ct);
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

    public async Task<ClientListResponse> QueryClientsAsync(ClientsPaginationRequest listRequest,
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

        var pageSize = totalClients - ((listRequest.Page - 1) * listRequest.PageSize);
        pageSize = Math.Min(pageSize, listRequest.PageSize);

        var listQuery = new ClientQuery(
            Page: listRequest.Page,
            PageSize: pageSize,
            SortBy: listRequest.SortBy ?? "ClientName",
            Order: listRequest.Order ?? "asc"
        );

        var clients = await _repository.QueryAsync(listQuery, filter, ct);

        var summaries = clients.Select(c => c.ToSummary()).ToList();

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
