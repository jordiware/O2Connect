using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Mappers;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Api.Repositories.Filters;
using O2Connect.Dto.Management.Users;
using System.Data;

namespace O2Connect.Api.Services.Management;

public interface IManagementUsersService
{
    Task<UserDetailResponse?> GetUserAsync(string userId,
                                           CancellationToken ct);
    Task<UserListResponse> QueryUsersAsync(EntityPagination pagination,
                                           UserFilter filter,
                                           CancellationToken ct);
    Task UpdateUserDisplayName(string userId,
                               string newDisplayName,
                               CancellationToken ct);
    Task UpdateUserImageUrl(string userId,
                            string? newImageUrl,
                            CancellationToken ct);
    Task UpdateUserRole(string userId,
                        string role,
                        CancellationToken ct);
    Task UpdateUserStatusAsync(string userId,
                               string status,
                               CancellationToken ct);
}

public class ManagementUsersService : IManagementUsersService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IUserConsentRepository _userConsentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ManagementUsersService> _logger;

    public ManagementUsersService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAuthorizationCodeRepository authorizationCodeRepository,
        IUserConsentRepository userConsentRepository,
        ICurrentUserService currentUserService,
        ILogger<ManagementUsersService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _authorizationCodeRepository = authorizationCodeRepository;
        _userConsentRepository = userConsentRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<UserDetailResponse?> GetUserAsync(string userId,
                                                        CancellationToken ct)
    {
        var user = await _userRepository.GetAsync(userId, ct);

        if (user is null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", userId);
            return null;
        }

        var response = user.ToDetailDto();

        return response;
    }

    public async Task<UserListResponse> QueryUsersAsync(EntityPagination pagination,
                                                        UserFilter filter,
                                                        CancellationToken ct)
    {
        var totalUsers = await _userRepository.CountAsync(filter, ct);

        if (totalUsers == 0)
        {
            return new UserListResponse
            {
                Items = [],
                TotalItems = 0,
                Page = pagination.Page,
                TotalPages = 0,
                PageSize = pagination.PageSize
            };
        }

        var pages = (int)Math.Ceiling((double)totalUsers / pagination.PageSize);

        if (pagination.Page > pages)
        {
            _logger.LogWarning("Requested page {Page} is out of range. Total pages: {Pages}", pagination.Page, pages);
            throw ApiException.BadRequest("invalid_requested_page", $"Page must be between 1 and {pages}");
        }

        var skip = (pagination.Page - 1) * pagination.PageSize;
        var remainingItems = totalUsers - skip;
        var pageSize = Math.Min(pagination.PageSize, remainingItems);

        pagination = pagination with { PageSize = pageSize };

        var users = await _userRepository.QueryAsync(pagination, filter, ct);

        var summaries = users.Select(c => c.ToSummaryDto()).ToList();

        var response = new UserListResponse
        {
            Items = summaries,
            TotalItems = totalUsers,
            Page = pagination.Page,
            TotalPages = pages,
            PageSize = pagination.PageSize
        };

        return response;
    }

    public async Task UpdateUserDisplayName(string userId,
                                            string newDisplayName,
                                            CancellationToken ct)
    {
        newDisplayName = newDisplayName.Trim();

        var user = await GetUserForUpdateAsync(userId, ct);

        var now = DateTimeOffset.UtcNow;

        user = user with
        {
            DisplayName = newDisplayName,
            LastModifiedAt = now
        };

        await _userRepository.StoreAsync(user, ct);
    }

    public async Task UpdateUserImageUrl(string userId,
                                         string? newImageUrl,
                                         CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(newImageUrl))
        {
            newImageUrl = newImageUrl.Trim();

            if (!Uri.TryCreate(newImageUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                _logger.LogWarning("Invalid image URL scheme for user {UserId}.", userId);
                throw ApiException.BadRequest("invalid_image_url", "Image URL must be HTTP or HTTPS.");
            }

            if (uri.IsLoopback || string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid image URL host for user {UserId}.", userId);
                throw ApiException.BadRequest("invalid_image_url", "Local URLs are not allowed.");
            }
        }
        else
        {
            newImageUrl = null;
        }

        var user = await GetUserForUpdateAsync(userId, ct);

        if (string.Equals(user.ImageUrl, newImageUrl, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("User already has image URL {imageUrl}. No update needed.",
                                   newImageUrl);
            return;
        }

        var now = DateTimeOffset.UtcNow;

        user = user with
        {
            ImageUrl = newImageUrl,
            LastModifiedAt = now
        };

        await _userRepository.StoreAsync(user, ct);
    }

    public async Task UpdateUserStatusAsync(string userId,
                                            string status,
                                            CancellationToken ct)
    {
        status = status.Trim();

        if (!Enum.TryParse<EntityStatus>(status, true, out var newStatus))
        {
            _logger.LogWarning("Invalid status value: {Status}", status);
            throw ApiException.BadRequest("invalid_requested_status", $"Invalid status value: {status}");
        }

        var user = await GetUserForUpdateAsync(userId, ct);

        if (newStatus == EntityStatus.Pending)
        {
            _logger.LogWarning("User {Username} can't be reverted to pending status.", user.Username);
            throw ApiException.BadRequest("invalid_requested_status",
                                          $"{user.Username} can't revert to 'pending' status");
        }

        if (user.Status == newStatus)
            return;

        var now = DateTimeOffset.UtcNow;

        user = user with
        {
            Status = newStatus,
            LastModifiedAt = now
        };

        if (newStatus == EntityStatus.Revoked)
            user = user with { RevokedAt = now };

        await _userRepository.StoreAsync(user, ct);

        if (user.Status != EntityStatus.Active)
        {
            await _refreshTokenRepository.RevokeSubjectAsync(user.Id, ct);
        }

        if (user.Status == EntityStatus.Revoked)
        {
            await _userConsentRepository.RevokeForUserAsync(user.Id, ct);
        }
    }

    public async Task UpdateUserRole(string userId,
                                     string role,
                                     CancellationToken ct)
    {
        role = role.Trim();

        if (!UserRole.TryParse(role, out var newRole))
        {
            _logger.LogWarning("Invalid requested role for user {UserId}.", userId);
            throw ApiException.BadRequest("invalid_role", $"Invalid requested role [{role}].");
        }

        var user = await GetUserForUpdateAsync(userId, ct);

        if (string.Equals(user.Role, newRole, StringComparison.Ordinal))
        {
            _logger.LogInformation("User already has role {newRole}. No update needed.",
                                   newRole);
            return;
        }

        var now = DateTimeOffset.UtcNow;

        user = user with
        {
            Role = newRole,
            Scopes = newRole.GetScopes().ToHashSet(),
            LastModifiedAt = now
        };

        await _userRepository.StoreAsync(user, ct);

        await _refreshTokenRepository.RevokeSubjectAsync(user.Id, ct);

        await _userConsentRepository.RevokeForUserAsync(user.Id, ct);
    }

    private async Task<User> GetUserForUpdateAsync(string userId, CancellationToken ct)
    {
        var user = await _userRepository.GetAsync(userId, ct);

        if (user is null)
        {
            _logger.LogInformation("User with ID '{UserId}' not found.", userId);
            throw ApiException.NotFound("user_not_found", $"User '{userId}' not found.");
        }

        if (user.Status == EntityStatus.Revoked)
        {
            _logger.LogInformation("User '{UserId}' is already revoked. No updates performed.", userId);
            throw ApiException.Conflict("user_revoked",
                                        $"User '{userId}' is revoked and updates cannot be performed.");
        }

        if (!_currentUserService.HasRole(UserRole.Admin) && !_currentUserService.HasRole(UserRole.Manager))
        {
            _logger.LogWarning("Unprivileged user {UserId} accessing to user {UserId}",
                               _currentUserService.UserId,
                               user.Id);
            throw ApiException.Unauthorized("restricted_access",
                                            "You don't have permissions for this action.");
        }

        return user;
    }
}
