using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.DataValidators;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Security;
using O2Connect.Api.Services;
using O2Connect.Dto.Management.Clients;

namespace O2Connect.Api.Controllers.Management;

[ApiController]
[Route("management/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientsQueryValidator _queryValidator;
    private readonly IClientsService _clientsController;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(
        IClientsQueryValidator queryValidator,
        IClientsService clientsController,
        ILogger<ClientsController> logger)
    {
        _queryValidator = queryValidator;
        _clientsController = clientsController;
        _logger = logger;
    }

    [HttpGet]
    [RequireScope(Scopes.Clients.Query)]
    public async Task<IActionResult> ListClients([FromQuery] ListClientsRequest request,
                                                 CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        if (!_queryValidator.ValidateListRequest(request, out var errorMessage))
        {
            _logger.LogWarning("Invalid request parameters: {ErrorMessage}", errorMessage);
            throw OAuthException.FromInvalidRequest(errorMessage);
        }

        var response = await _clientsController.ListClientsAsync(request, ct);

        return Ok(response);
    }
}
