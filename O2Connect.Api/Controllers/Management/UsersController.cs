using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Mappers;
using O2Connect.Api.Repositories.Filters;
using O2Connect.Api.Security;
using O2Connect.Api.Services.Management;
using O2Connect.Dto.Management;
using O2Connect.Dto.Management.Users;

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

    [HttpGet]
    [RequireScope(Scopes.Users.Query)]
    public async Task<IActionResult> ListUsers([FromQuery] QueryPaginationRequest paginationRequest,
                                               CancellationToken ct)
    {
        var pagination = paginationRequest.ToPagination();

        var response = await _usersService.QueryUsersAsync(pagination, UserFilter.Empty, ct);

        return Ok(response);
    }

    [HttpPost("search")]
    [RequireScope(Scopes.Users.Query)]
    public async Task<IActionResult> SearchUsers([FromBody] UsersSearchRequest searchRequest,
                                                 CancellationToken ct)
    {
        var pagination = searchRequest.Pagination.ToPagination();
        var filter = searchRequest.Filters.ToFilter();

        var response = await _usersService.QueryUsersAsync(pagination, filter, ct);

        return Ok(response);
    }

    [HttpGet("{userId}")]
    [RequireScope(Scopes.Clients.Read)]
    public async Task<IActionResult> GetClient([FromRoute] string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("User ID is missing in the request.");
            throw ApiException.BadRequest("invalid_request_params", "User ID is required.");
        }

        var response = await _usersService.GetUserAsync(userId, ct);

        if (response == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPatch("{userId}/display_name")]
    [RequireScope(Scopes.Users.Write)]
    public async Task<IActionResult> UpdateDisplayName([FromRoute] string userId,
                                                       [FromBody] UpdateUserDisplayNameRequest request,
                                                       CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("User ID is missing in the request.");
            throw ApiException.BadRequest("invalid_request_params", "User ID is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            _logger.LogWarning("Requested display name is empty.");
            throw ApiException.BadRequest("invalid_request_params", "Display name is required.");
        }

        _logger.LogInformation("Updating user {UserId} display name to [{DisplayName}]",
                               userId,
                               request.DisplayName);

        await _usersService.UpdateUserDisplayNameAsync(userId, request.DisplayName, ct);

        return NoContent();
    }

    [HttpPatch("{userId}/image_url")]
    [RequireScope(Scopes.Users.Write)]
    public async Task<IActionResult> UpdateImageUrl([FromRoute] string userId,
                                                    [FromBody] UpdateUserImageUrlRequest request,
                                                    CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("User ID is missing in the request.");
            throw ApiException.BadRequest("invalid_request_params", "User ID is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            _logger.LogWarning("Requested image URL is empty.");
            throw ApiException.BadRequest("invalid_request_params", "Image URL is required.");
        }

        _logger.LogInformation("Updating user {UserId} image URL to [{ImageUrl}]",
                               userId,
                               request.ImageUrl);

        await _usersService.UpdateUserImageUrlAsync(userId, request.ImageUrl, ct);

        return NoContent();
    }

    [HttpPatch("{userId}/role")]
    [RequireScope(Scopes.Users.Write)]
    public async Task<IActionResult> UpdateRole([FromRoute] string userId,
                                                [FromBody] UpdateUserRoleRequest request,
                                                CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("User ID is missing in the request.");
            throw ApiException.BadRequest("invalid_request_params", "User ID is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            _logger.LogWarning("Role is missing in request.");
            throw ApiException.BadRequest("invalid_request_params", "Role is required.");
        }

        _logger.LogInformation("Updating user {UserId} role to {Role}",
                               userId,
                               request.Role);

        await _usersService.UpdateUserRoleAsync(userId, request.Role, ct);

        return NoContent();
    }

    [HttpPatch("{userId}/status")]
    [RequireScope(Scopes.Users.Write)]
    public async Task<IActionResult> UpdateStatus([FromRoute] string userId,
                                                  [FromBody] UpdateUserStatusRequest request,
                                                  CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("User ID is missing in the request.");
            throw ApiException.BadRequest("invalid_request_params", "User ID is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            _logger.LogWarning("Status is missing in request.");
            throw ApiException.BadRequest("invalid_request_params", "Status is required.");
        }

        _logger.LogInformation("Updating user {UserId} status to {Status}",
                               userId,
                               request.Status);

        await _usersService.UpdateUserStatusAsync(userId, request.Status, ct);

        return NoContent();
    }
}
