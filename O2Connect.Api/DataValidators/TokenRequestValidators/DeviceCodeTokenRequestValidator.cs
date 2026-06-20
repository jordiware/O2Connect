using O2Connect.Api.Crypto;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.OidcOAuth.Connect;

namespace O2Connect.Api.DataValidators.TokenRequestValidators;

public class DeviceCodeTokenRequestValidator : ITokenRequestValidator
{
    private readonly IDeviceAuthorizationRepository _deviceAuthorizationRepository;
    private readonly ISecretHasher _secretHasher;

    public GrantType GrantType => GrantType.DeviceCode;

    public DeviceCodeTokenRequestValidator(
        IDeviceAuthorizationRepository deviceAuthorizationRepository,
        ISecretHasher secretHasher)
    {
        _deviceAuthorizationRepository = deviceAuthorizationRepository;
        _secretHasher = secretHasher;
    }

    public async Task<TokenRequestContext> ValidateAsync(TokenRequest request,
                                                         Client client,
                                                         ClientAuthenticationMethod method,
                                                         CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceCode))
            throw OAuthException.FromInvalidRequest();

        var deviceCodeHash = _secretHasher.FastHash(request.DeviceCode);

        var deviceAuth = await _deviceAuthorizationRepository.GetAsync(deviceCodeHash, ct);
        if (deviceAuth is null)
            throw OAuthException.FromInvalidGrant();

        if (!string.Equals(deviceAuth.ClientId, client.Id, StringComparison.Ordinal))
            throw OAuthException.FromInvalidGrant();

        var now = DateTimeOffset.UtcNow;

        if (deviceAuth.ExpiresAtUtc <= now)
            throw OAuthException.FromInvalidGrant();

        if (deviceAuth.LastPollAtUtc is not null)
        {
            var elapsed = (now - deviceAuth.LastPollAtUtc.Value).TotalSeconds;

            if (elapsed < deviceAuth.Interval)
            {
                deviceAuth = deviceAuth with { LastPollAtUtc = now };

                await _deviceAuthorizationRepository.StoreAsync(deviceAuth, ct);

                throw OAuthException.FromSlowDown();
            }
        }

        deviceAuth = deviceAuth with
        {
            LastPollAtUtc = now,
            PollCount = deviceAuth.PollCount + 1
        };
        await _deviceAuthorizationRepository.StoreAsync(deviceAuth, ct);

        if (deviceAuth.IsDenied)
            throw OAuthException.FromAccessDenied();

        if (!deviceAuth.IsAuthorized)
            throw OAuthException.FromAuthorizationPending();

        if (deviceAuth.UserId is null)
            throw OAuthException.FromInvalidGrant();

        return new TokenRequestContext
        {
            TokenRequest = request,
            Client = client,
            GrantType = GrantType.DeviceCode,
            ClientAuthenticationMethod = method,
            Scopes = deviceAuth.Scopes.ToHashSet(),

            UserId = deviceAuth.UserId,
            DeviceAuthorization = deviceAuth
        };
    }
}
