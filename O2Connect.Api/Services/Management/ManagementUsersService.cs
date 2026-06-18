using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Mappers;
using O2Connect.Api.Repositories;
using O2Connect.Api.Repositories.Filters;
using O2Connect.Dto.Management.Users;

namespace O2Connect.Api.Services.Management;

public interface IManagementUsersService
{
    Task<UserDetailResponse?> GetUserAsync(string userId,
                                           CancellationToken ct);
    Task<UserListResponse> QueryUsersAsync(EntityPagination pagination,
                                           UserFilter filter,
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

    public async Task<UserDetailResponse?> GetUserAsync(string userId, CancellationToken ct)
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
}
