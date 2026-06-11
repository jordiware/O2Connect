using O2Connect.Api.Crypto;
using O2Connect.Dto.Responses;
using System.Security.Cryptography;

namespace O2Connect.Api.Services;

public interface IDeviceConnectService
{
    Task<DeviceAuthorizationResponse> CreateAsync(string clientId, string scope);
}

public sealed class DeviceConnectService : IDeviceConnectService
{
    private readonly ISecretHasher _secretHasher;

    public DeviceConnectService(
        ISecretHasher secretHasher)
    {
        _secretHasher = secretHasher;
    }

    public async Task<DeviceAuthorizationResponse> CreateAsync(string clientId, string scope)
    {
        var deviceCode = SecureCodeGenerator.GenerateBase64UrlToken(64);
        var userCode = GenerateUserCode();

        var expiresIn = 600; // 10 minutes
        var interval = 5;

        // TODO: store hashed codes in DB

        return new DeviceAuthorizationResponse
        {
            DeviceCode = deviceCode,
            UserCode = userCode,
            VerificationUri = "https://your-auth-server/connect/device",
            VerificationUriComplete = $"https://your-auth-server/connect/device?user_code={userCode}",
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
