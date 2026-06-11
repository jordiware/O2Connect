using Microsoft.Extensions.Options;
using O2Connect.Api.Crypto;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Options;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Responses;
using System.Security.Cryptography;

namespace O2Connect.Api.Services;

public interface IDeviceConnectService
{
    Task<DeviceAuthorizationResponse> CreateAsync(string clientId,
                                                  string scope,
                                                  CancellationToken ct);
}

public sealed class DeviceConnectService : IDeviceConnectService
{
    private readonly IDeviceAuthorizationRepository _deviceAuthorizationRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ISecretHasher _secretHasher;
    private readonly JwtOptions _jwtOptions;

    public DeviceConnectService(
        IDeviceAuthorizationRepository deviceAuthorizationRepository,
        IClientRepository clientRepository,
        ISecretHasher secretHasher,
        IOptions<JwtOptions> options)
    {
        _deviceAuthorizationRepository = deviceAuthorizationRepository;
        _clientRepository = clientRepository;
        _secretHasher = secretHasher;
        _jwtOptions = options.Value;
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
            Scope = scope,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddSeconds(expiresIn),
            PollCount = 0
        };

        await _deviceAuthorizationRepository.StoreAsync(authorization, ct);

        var verificationUri = $"{_jwtOptions.Issuer}/connect/device";

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
