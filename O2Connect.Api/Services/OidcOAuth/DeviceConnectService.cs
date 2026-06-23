using O2Connect.Api.Config;
using O2Connect.Api.Crypto;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.OidcOAuth.Connect;
using System.Security.Cryptography;

namespace O2Connect.Api.Services.OidcOAuth;

public interface IDeviceConnectService
{
    Task<DeviceAuthorizationResponse> CreateAsync(string clientId,
                                                  string scope,
                                                  CancellationToken ct);
    Task ConsumeUserCodeAsync(string userCode, bool approved, string userId, CancellationToken ct);
    Task<DeviceStatusResponse> GetStatusAsync(string userCode, CancellationToken ct);
}

public sealed class DeviceConnectService : IDeviceConnectService
{
    private readonly IDeviceAuthorizationRepository _deviceAuthorizationRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ISecretHasher _secretHasher;
    private readonly IJwtConfig _jwtConfig;

    public DeviceConnectService(
        IDeviceAuthorizationRepository deviceAuthorizationRepository,
        IClientRepository clientRepository,
        ISecretHasher secretHasher,
        IJwtConfig jwtConfig)
    {
        _deviceAuthorizationRepository = deviceAuthorizationRepository;
        _clientRepository = clientRepository;
        _secretHasher = secretHasher;
        _jwtConfig = jwtConfig;
    }

    public async Task<DeviceAuthorizationResponse> CreateAsync(string clientId,
                                                               string scope,
                                                               CancellationToken ct)
    {
        var client = await _clientRepository.GetAsync(clientId, ct);

        if (client == null)
            throw OAuthException.FromInvalidClient();

        if (!client.AllowedGrantTypes.Contains(GrantType.DeviceCode.Value))
            throw OAuthException.FromUnauthorizedClient();

        var requestedScopes = ValueSet.FromDataString(scope, ' ');
        if (!requestedScopes.IsSubsetOf(client.AllowedScopes))
            throw OAuthException.FromInvalidScope();

        var deviceCode = SecureCodeGenerator.GenerateBase64UrlToken(64);
        var userCode = GenerateUserCode();

        var now = DateTimeOffset.UtcNow;
        var expiresIn = 600; // 10 minutes
        var interval = 5;

        var authorization = new DeviceAuthorization
        {
            DeviceCodeHash = _secretHasher.FastHash(deviceCode),
            UserCodeHash = _secretHasher.FastHash(userCode),
            ClientId = clientId,
            Scopes = requestedScopes.Values.ToArray(),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddSeconds(expiresIn),
            PollCount = 0,
            Interval = interval
        };

        await _deviceAuthorizationRepository.StoreAsync(authorization, ct);

        var verificationUri = $"{_jwtConfig.Issuer}/connect/device";

        return new DeviceAuthorizationResponse
        {
            DeviceCode = deviceCode,
            UserCode = userCode,
            VerificationUri = verificationUri,
            VerificationUriComplete = $"{verificationUri}?user_code={userCode}",
            ExpiresIn = expiresIn,
            Interval = interval
        };
    }

    public async Task ConsumeUserCodeAsync(string userCode, bool approved, string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw OAuthException.FromInvalidRequest();

        var userCodeHash = _secretHasher.FastHash(userCode);

        var deviceAuth = await _deviceAuthorizationRepository.GetByUserCodeAsync(userCodeHash, ct);
        if (deviceAuth is null)
            throw OAuthException.FromInvalidGrant();

        var now = DateTimeOffset.UtcNow;

        if (deviceAuth.ExpiresAtUtc <= now)
            throw OAuthException.FromInvalidGrant();

        if (!deviceAuth.IsAuthorized || !deviceAuth.IsDenied)
            throw OAuthException.FromInvalidGrant();

        if (deviceAuth.IsConsumed)
            throw OAuthException.FromInvalidGrant();

        if (approved)
        {
            deviceAuth = deviceAuth with
            {
                UserId = userId,
                AuthorizedAtUtc = now
            };
        }
        else
        {
            deviceAuth = deviceAuth with
            {
                DeniedAtUtc = now
            };
        }

        await _deviceAuthorizationRepository.StoreAsync(deviceAuth, ct);
    }

    public async Task<DeviceStatusResponse> GetStatusAsync(string userCode, CancellationToken ct)
    {
        var userCodeHash = _secretHasher.FastHash(userCode);

        var deviceAuth = await _deviceAuthorizationRepository.GetByUserCodeAsync(userCodeHash, ct);

        var now = DateTimeOffset.UtcNow;

        var status = deviceAuth switch
        {
            null => "pending",
            _ when deviceAuth.ExpiresAtUtc <= now => "expired",
            _ when deviceAuth.IsDenied => "denied",
            _ when deviceAuth.IsAuthorized => "approved",
            _ => "pending"
        };

        var expiresIn = deviceAuth is null || deviceAuth.ExpiresAtUtc <= now
                        ? -1
                        : (int)(deviceAuth.ExpiresAtUtc - now).TotalSeconds;

        return new DeviceStatusResponse
        {
            Status = status,
            ExpiresIn = expiresIn
        };
    }

    private static string GenerateUserCode()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = RandomNumberGenerator.GetBytes(8);

        Span<char> chars = stackalloc char[8];

        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = alphabet[random[i] % alphabet.Length];
        }

        return $"{new string(chars[..4])}-{new string(chars[4..])}";
    }
}
