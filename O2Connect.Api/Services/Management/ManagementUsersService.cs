using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;

namespace O2Connect.Api.Services.Management;

public interface IManagementUsersService
{
    Task<User?> GetUserAsync(string userId, CancellationToken ct);
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

    public async Task<User?> GetUserAsync(string userId, CancellationToken ct)
    {
        var user = await _userRepository.GetAsync(userId, ct);

        if (user is null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", userId);
            return null;
        }

        var response = user;

        return response;
    }

}
