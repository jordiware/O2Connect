using O2Connect.Api.Crypto;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services;

public interface IDeviceAuthorizationService
{
    Task<DeviceAuthorizationResponse> CreateAsync(string clientId, string scope);
}

public sealed class DeviceAuthorizationService : IDeviceAuthorizationService
{
    public Task<DeviceAuthorizationResponse> CreateAsync(string clientId, string scope)
    {
        var deviceCode = SecureCodeGenerator.GenerateBase64UrlToken(64);
        var userCode = SecureCodeGenerator.GenerateBase64UrlToken(16);

        var expiresIn = 600; // 10 minutes
        var interval = 5;

        // TODO: store hashed codes in DB

        return Task.FromResult(new DeviceAuthorizationResponse
        {
            DeviceCode = deviceCode,
            UserCode = userCode,
            VerificationUri = "https://your-auth-server/connect/device",
            VerificationUriComplete = $"https://your-auth-server/connect/device?user_code={userCode}",
            ExpiresIn = expiresIn,
            Interval = interval
        });
    }
}
