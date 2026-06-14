using O2Connect.Api.Exceptions;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Management.Clients;

namespace O2Connect.Api.Services;

public interface IClientsService
{
    Task<ClientListResponse> ListClientsAsync(ListClientsRequest request, CancellationToken ct);
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
                Total = 0,
                Page = request.Page,
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

        var listQuery = new ClientListQuery(
            Page: request.Page,
            PageSize: pageSize,
            SortBy: request.SortBy ?? "ClientName",
            Order: request.Order ?? "asc"
        );

        var clients = await _repository.ListAsync(listQuery, ct);

        var summaries = clients.Select(client => new ClientSummaryDto
        {
            Id = client.ClientId,
            Name = client.ClientName,
            Enabled = client.IsActive,
        }).ToList();

        var response = new ClientListResponse
        {
            Items = summaries,
            Total = totalClients,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return response;
    }
}
