using O2Connect.Api.Models.Mappers;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Management.Users;

namespace O2Connect.Api.Services.Management;

public interface IManagementProfileService
{
    Task<UserDetailResponse?> GetMeAsync(CancellationToken ct);
}

public class ManagementProfileService : IManagementProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserConsentRepository _userConsentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ManagementUsersService> _logger;

    public ManagementProfileService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUserConsentRepository userConsentRepository,
        ICurrentUserService currentUserService,
        ILogger<ManagementUsersService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _userConsentRepository = userConsentRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<UserDetailResponse?> GetMeAsync(CancellationToken ct)
    {
        var user = await GetCurrentUserAsync(ct);

        var response = user.ToDetailDto();

        return response;
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        var user = await _userRepository.GetAsync(userId!, ct);

        if (user is null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", userId);
            throw new InvalidOperationException();
        }

        return user;
    }

    private string GetCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated)
        {
            _logger.LogWarning("User is not authenticated.");
            throw new InvalidOperationException();
        }

        var userId = _currentUserService.UserId;

        return userId!;
    }
}
