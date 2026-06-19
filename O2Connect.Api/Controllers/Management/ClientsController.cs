using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.DataValidators;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Mappers;
using O2Connect.Api.Repositories.Filters;
using O2Connect.Api.Security;
using O2Connect.Api.Services.Management;
using O2Connect.Dto.Management;
using O2Connect.Dto.Management.Clients;

namespace O2Connect.Api.Controllers.Management;

[ApiController]
[Route("management/clients")]
public class ClientsController : ControllerBase
{
    private readonly IManagementClientsService _clientsService;
    private readonly IPaginationQueryValidator _paginationValidator;
    private readonly IClientsQueryValidator _queryValidator;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(
        IManagementClientsService clientsService,
        IPaginationQueryValidator paginationValidator,
        IClientsQueryValidator queryValidator,
        ILogger<ClientsController> logger)
    {
        _clientsService = clientsService;
        _paginationValidator = paginationValidator;
        _queryValidator = queryValidator;
        _logger = logger;
    }

    [HttpGet]
    [RequireScope(Scopes.Clients.Query)]
    public async Task<IActionResult> ListClients([FromQuery] QueryPaginationRequest paginationRequest,
                                                 CancellationToken ct)
    {
        if (paginationRequest is null || !ModelState.IsValid)
        {
            _logger.LogWarning("Missing or malformed request body.");
            throw ApiException.BadRequest("invalid_request_params", "Missing or malformed request body.");
        }

        if (!_paginationValidator.ValidatePaginationRequest(paginationRequest, out var errorMessage))
        {
            _logger.LogWarning("Invalid request parameters: {ErrorMessage}", errorMessage);
            throw ApiException.BadRequest("invalid_request_params", errorMessage);
        }
        
        var pagination = paginationRequest.ToEntityPagination();

        var response = await _clientsService.QueryClientsAsync(pagination, ClientFilter.Empty, ct);

        return Ok(response);
    }

    [HttpPost("search")]
    [RequireScope(Scopes.Clients.Query)]
    public async Task<IActionResult> SearchClients([FromBody] ClientsSearchRequest searchRequest,
                                                   CancellationToken ct)
    {
        if (searchRequest is null || !ModelState.IsValid)
        {
            _logger.LogWarning("Missing or malformed request body.");
            throw ApiException.BadRequest("invalid_request_params", "Missing or malformed request body.");
        }

        if (!_paginationValidator.ValidatePaginationRequest(searchRequest.Pagination,
                                                            out var paginationErrorMessage))
        {
            _logger.LogWarning("Invalid request parameters: {ErrorMessage}", paginationErrorMessage);
            throw ApiException.BadRequest("invalid_request_params", paginationErrorMessage);
        }

        if (!_queryValidator.ValidateSearchRequest(searchRequest, out var queryErrorMessage))
        {
            _logger.LogWarning("Invalid search parameters: {ErrorMessage}", queryErrorMessage);
            throw ApiException.BadRequest("invalid_request_params", queryErrorMessage);
        }

        var pagination = searchRequest.Pagination.ToEntityPagination();
        var filter = searchRequest.Filter.ToFilter();

        var response = await _clientsService.QueryClientsAsync(pagination, filter, ct);

        return Ok(response);
    }

    [HttpGet("{clientId}")]
    [RequireScope(Scopes.Clients.Read)]
    public async Task<IActionResult> GetClient([FromRoute] string clientId,
                                               CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Client ID is missing in the request.");
            throw ApiException.BadRequest("invalid_request_params", "Client ID is required.");
        }

        var response = await _clientsService.GetClientAsync(clientId, ct);

        if (response == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPatch("{clientId}/display_name")]
    [RequireScope(Scopes.Clients.Write)]
    public async Task<IActionResult> UpdateDisplayName([FromRoute] string clientId,
                                                      [FromBody] UpdateClientDisplayNameRequest request,
                                                      CancellationToken ct)
    {
        if (request is null || !ModelState.IsValid)
        {
            _logger.LogWarning("Missing or malformed request body.");
            throw ApiException.BadRequest("invalid_request_params", "Missing or malformed request body.");
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Client ID is missing in the request.");
            throw ApiException.BadRequest("invalid_request_params", "Client ID is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            _logger.LogWarning("Requested display name is empty.");
            throw ApiException.BadRequest("invalid_request_params", "Display name is required.");
        }

        _logger.LogInformation("Updating client {ClientId} display name to [{DisplayName}]",
                               clientId,
                               request.DisplayName);

        await _clientsService.UpdateClientDisplayNameAsync(clientId, request.DisplayName, ct);

        return NoContent();
    }

    [HttpPatch("{clientId}/image_url")]
    [RequireScope(Scopes.Clients.Write)]
    public async Task<IActionResult> UpdateImageUrl([FromRoute] string clientId,
                                                    [FromBody] UpdateClientImageUrlRequest request,
                                                    CancellationToken ct)
    {
        if (request is null || !ModelState.IsValid)
        {
            _logger.LogWarning("Missing or malformed request body.");
            throw ApiException.BadRequest("invalid_request_params", "Missing or malformed request body.");
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Client ID is missing in the request.");
            throw ApiException.BadRequest("invalid_request_params", "Client ID is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            _logger.LogWarning("Requested image URL is empty.");
            throw ApiException.BadRequest("invalid_request_params", "Image URL is required.");
        }

        _logger.LogInformation("Updating client {ClientId} image URL to [{ImageUrl}]",
                               clientId,
                               request.ImageUrl);

        await _clientsService.UpdateClientImageUrlAsync(clientId, request.ImageUrl, ct);

        return NoContent();
    }

    [HttpPatch("{clientId}/redirect_uris")]
    [RequireScope(Scopes.Clients.Write)]
    public async Task<IActionResult> UpdateRedirectUris([FromRoute] string clientId,
                                                        [FromBody] UpdateClientRedirectUrisRequest request,
                                                        CancellationToken ct)
    {
        if (request is null || !ModelState.IsValid)
        {
            _logger.LogWarning("Missing or malformed request body.");
            throw ApiException.BadRequest("invalid_request_params", "Missing or malformed request body.");
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Client ID is missing in the request.");
            throw ApiException.BadRequest("invalid_request_params", "Client ID is required.");
        }

        if (request.RedirectUris is null)
        {
            _logger.LogWarning("Redirect URIs are missing in request.");
            throw ApiException.BadRequest("invalid_request_params", "Redirect URIs is required.");
        }

        _logger.LogInformation("Updating client {ClientId} redirect URIs to [{RedirectUris}]",
                               clientId,
                               string.Join(' ', request.RedirectUris));

        await _clientsService.UpdateClientRedirectUrisAsync(clientId, request.RedirectUris, ct);

        return NoContent();
    }

    [HttpPatch("{clientId}/scopes")]
    [RequireScope(Scopes.Clients.Write)]
    public async Task<IActionResult> UpdateScopes([FromRoute] string clientId,
                                                  [FromBody] UpdateClientScopesRequest request,
                                                  CancellationToken ct)
    {
        if (request is null || !ModelState.IsValid)
        {
            _logger.LogWarning("Missing or malformed request body.");
            throw ApiException.BadRequest("invalid_request_params", "Missing or malformed request body.");
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Client ID is missing in the request.");
            throw ApiException.BadRequest("invalid_request_params", "Client ID is required.");
        }

        if (request.Scopes is null)
        {
            _logger.LogWarning("Scopes are missing in request.");
            throw ApiException.BadRequest("invalid_request_params", "Scopes is required.");
        }

        _logger.LogInformation("Updating client {ClientId} scopes to [{Scopes}]",
                               clientId,
                               string.Join(' ', request.Scopes));

        await _clientsService.UpdateClientScopesAsync(clientId, request.Scopes, ct);

        return NoContent();
    }

    [HttpPatch("{clientId}/status")]
    [RequireScope(Scopes.Clients.Write)]
    public async Task<IActionResult> UpdateStatus([FromRoute] string clientId,
                                                  [FromBody] UpdateClientStatusRequest request,
                                                  CancellationToken ct)
    {
        if (request is null || !ModelState.IsValid)
        {
            _logger.LogWarning("Missing or malformed request body.");
            throw ApiException.BadRequest("invalid_request_params", "Missing or malformed request body.");
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Client ID is missing in the request.");
            throw ApiException.BadRequest("invalid_request_params", "Client ID is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            _logger.LogWarning("Status is missing in request.");
            throw ApiException.BadRequest("invalid_request_params", "Status is required.");
        }

        _logger.LogInformation("Updating client {ClientId} status to {Status}",
                               clientId,
                               request.Status);

        await _clientsService.UpdateClientStatusAsync(clientId, request.Status, ct);

        return NoContent();
    }
}
