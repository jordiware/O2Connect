using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Models;
using O2Connect.Api.Security;
using O2Connect.Dto.Management.Clients;

namespace O2Connect.Api.Controllers.Management;

[ApiController]
[Route("management/clients")]
public class ClientsController : ControllerBase
{
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(
        ILogger<ClientsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [RequireScope(Scopes.Clients.Query)]
    public async Task<IActionResult> ListClients([FromQuery] ListClientsRequest request,
                                                 CancellationToken ct)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
