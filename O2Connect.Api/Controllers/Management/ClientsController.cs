using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.DataValidators;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Mappers;
using O2Connect.Api.Repositories.Filters;
using O2Connect.Api.Security;
using O2Connect.Api.Services;
using O2Connect.Dto.Management.Clients;

namespace O2Connect.Api.Controllers.Management;

[ApiController]
[Route("management/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientsQueryValidator _queryValidator;
    private readonly IClientsService _clientsService;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(
        IClientsQueryValidator queryValidator,
        IClientsService clientsService,
        ILogger<ClientsController> logger)
    {
        _queryValidator = queryValidator;
        _clientsService = clientsService;
        _logger = logger;
    }

    [HttpGet]
    [RequireScope(Scopes.Clients.Query)]
    public async Task<IActionResult> ListClients([FromQuery] ClientsPaginationRequest paginationRequest,
                                                 CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        if (!_queryValidator.ValidatePaginationRequest(paginationRequest, out var errorMessage))
        {
            _logger.LogWarning("Invalid request parameters: {ErrorMessage}", errorMessage);
            throw OAuthException.FromInvalidRequest(errorMessage);
        }

        var response = await _clientsService.QueryClientsAsync(paginationRequest, ClientFilter.Empty, ct);

        return Ok(response);
    }

    [HttpPost("search")]
    [RequireScope(Scopes.Clients.Query)]
    public async Task<IActionResult> SearchClients([FromBody] ClientSearchRequest searchRequest,
                                                   CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        if (!_queryValidator.ValidateSearchRequest(searchRequest, out var errorMessage))
        {
            _logger.LogWarning("Invalid search parameters: {ErrorMessage}", errorMessage);
            throw OAuthException.FromInvalidRequest(errorMessage);
        }

        var paginationRequest = searchRequest.Pagination.ToPaginationRequest();
        var filterRequest = searchRequest.Filters.ToFilter();

        var response = await _clientsService.QueryClientsAsync(paginationRequest, filterRequest, ct);

        return Ok(response);
    }

    [HttpGet("{clientId}")]
    [RequireScope(Scopes.Clients.Read)]
    public async Task<IActionResult> GetClient([FromRoute] string clientId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Client ID is missing in the request.");
            throw OAuthException.FromInvalidRequest("Client ID is required.");
        }
        
        var response = await _clientsService.GetClientAsync(clientId, ct);
        
        if (response == null)
            return NotFound();
        
        return Ok(response);
    }
}
