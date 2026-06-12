using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Api.Controllers.Management;

[ApiController]
[Route("management/users")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        ILogger<UsersController> logger)
    {
        _logger = logger;
    }
}
