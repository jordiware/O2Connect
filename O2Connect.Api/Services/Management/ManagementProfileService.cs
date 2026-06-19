using O2Connect.Api.Crypto;
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
    Task UpdatePasswordAsync(string oldPassword,
                             string newPassword,
                             CancellationToken ct);
}

public class ManagementProfileService : IManagementProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserConsentRepository _userConsentRepository;
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISecretHasher _secretHasher;
    private readonly ILogger<ManagementUsersService> _logger;

    public ManagementProfileService(
        IUserRepository userRepository,
        IClientRepository clientRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUserConsentRepository userConsentRepository,
        IAuthorizationCodeRepository authorizationCodeRepository,
        ICurrentUserService currentUserService,
        ISecretHasher secretHasher,
        ILogger<ManagementUsersService> logger)
    {
        _userRepository = userRepository;
        _clientRepository = clientRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _userConsentRepository = userConsentRepository;
        _authorizationCodeRepository = authorizationCodeRepository;
        _currentUserService = currentUserService;
        _secretHasher = secretHasher;
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

    public async Task UpdatePasswordAsync(string oldPassword,
                                          string newPassword,
                                          CancellationToken ct)
    {
        if (!ValidatePassword(newPassword))
        {
            _logger.LogWarning("Malformed new password.");
            throw ApiException.BadRequest("invalid_request_params", $"Malformed new password.");
        }

        var user = await GetCurrentUserAsync(ct);

        if (!_secretHasher.Verify(oldPassword, user.PasswordHash))
        {
            _logger.LogWarning("Current password mismatch.");
            throw ApiException.Unauthorized("invalid_credentials", "Invalid credentials.");
        }

        if (!_secretHasher.TryHash(newPassword, out var passwordHash))
        {
            _logger.LogWarning("Unexpected hashing error.");
            throw ApiException.ServerError("unexpected_error", "Unexpected error.");
        }

        user = user with { PasswordHash = passwordHash };

        await _userRepository.StoreAsync(user, ct);

        await _refreshTokenRepository.RevokeSubjectAsync(user.Id, ct);

        await _authorizationCodeRepository.RevokeForSubjectAsync(user.Id, ct);
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

    private static bool ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (password.Length < 8 || password.Length > 128)
            return false;

        bool hasLower = false;
        bool hasUpper = false;
        bool hasDigit = false;

        foreach (var c in password)
        {
            if (char.IsLower(c)) hasLower = true;
            else if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsDigit(c)) hasDigit = true;
        }

        if (!hasLower || !hasUpper || !hasDigit)
            return false;

        return true;
    }
}
