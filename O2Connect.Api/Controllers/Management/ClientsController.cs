using Microsoft.AspNetCore.Mvc;

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
}
