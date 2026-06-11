using O2Connect.Api.DataFactories;
using O2Connect.Api.DataFactories.RequestModels;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Context;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.DataHandlers.TokenGrantHandlers;

public class DeviceCodeGrantHandler : ITokenGrantHandler
{
    private readonly IDeviceAuthorizationRepository _deviceAuthorizationRepository;
    private readonly ITokenFactory _tokenFactory;

    public GrantType GrantType => GrantType.DeviceCode;

    public DeviceCodeGrantHandler(
        IDeviceAuthorizationRepository deviceAuthorizationRepository,
        ITokenFactory tokenFactory)
    {
        _deviceAuthorizationRepository = deviceAuthorizationRepository;
        _tokenFactory = tokenFactory;
    }

    public async Task<TokenResponse> HandleAsync(TokenRequestContext context, CancellationToken ct)
    {
        if (context.UserId is null)
            throw OAuthException.FromInvalidGrant();

        if (context.DeviceAuthorization is null)
            throw OAuthException.FromInvalidGrant();

        var now = DateTimeOffset.UtcNow;

        var deviceAuth = context.DeviceAuthorization;

        if (deviceAuth.IsConsumed)
            throw OAuthException.FromInvalidGrant();

        deviceAuth = deviceAuth with { ConsumedAtUtc = now };

        await _deviceAuthorizationRepository.StoreAsync(deviceAuth, ct);

        var response = await _tokenFactory.GenerateAsync(new JwtTokenFactoryRequest
        {
            ClientId = context.Client.ClientId,
            Scopes = context.Scopes,
            Subject = context.UserId,
            AdditionalClaims = new Dictionary<string, object>
            {
                ["amr"] = "device",
                ["auth_method"] = "device_code"
            }
        }, ct);

        return response;
    }
}
