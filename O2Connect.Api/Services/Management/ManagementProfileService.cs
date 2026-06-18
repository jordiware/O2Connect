using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Mappers;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Management.Clients;
using O2Connect.Dto.Management.Users;

namespace O2Connect.Api.Services.Management;

public interface IManagementProfileService
{
    Task<UserDetailResponse> GetMeAsync(CancellationToken ct);
    Task<IReadOnlyList<ClientSummaryResponse>> GetConsentedClientsAsync(CancellationToken ct);
}

public class ManagementProfileService : IManagementProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserConsentRepository _userConsentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ManagementUsersService> _logger;

    public ManagementProfileService(
        IUserRepository userRepository,
        IClientRepository clientRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUserConsentRepository userConsentRepository,
        ICurrentUserService currentUserService,
        ILogger<ManagementUsersService> logger)
    {
        _userRepository = userRepository;
        _clientRepository = clientRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _userConsentRepository = userConsentRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<UserDetailResponse> GetMeAsync(CancellationToken ct)
    {
        var user = await GetCurrentUserAsync(ct);

        var response = user.ToDetailDto();

        return response;
    }

    public async Task<IReadOnlyList<ClientSummaryResponse>> GetConsentedClientsAsync(CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        var consents = await _userConsentRepository.GetForUserAsync(userId, ct);

        var clients = new List<ClientSummaryResponse>();

        foreach (var consent in consents)
        {
            var client = await _clientRepository.GetAsync(consent.ClientId, ct);

            if (client != null)
                clients.Add(client.ToSummaryDto());
            else
                await _userConsentRepository.DeleteAsync(userId, consent.ClientId, ct);
        }

        return clients;
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        var user = await _userRepository.GetAsync(userId, ct);

        if (user is null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", userId);
            throw ApiException.NotFound("user_not_found", $"User with ID {userId} not found.");
        }

        return user;
    }

    private string GetCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            _logger.LogWarning("User is not authenticated.");
            throw ApiException.Unauthorized("user_not_authenticated", "User is not authenticated.");
        }

        var userId = _currentUserService.UserId;

        return userId;
    }
}
