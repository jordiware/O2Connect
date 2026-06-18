using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Services.Management;

namespace O2Connect.Api.Controllers.Management;

[ApiController]
[Route("management/users")]
public class UsersController : ControllerBase
{
    private readonly IManagementUsersService _usersService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IManagementUsersService usersService,
        ILogger<UsersController> logger)
    {
        _usersService = usersService;
        _logger = logger;
    }
}
