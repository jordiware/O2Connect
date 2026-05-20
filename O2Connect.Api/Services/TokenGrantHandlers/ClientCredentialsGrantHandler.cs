using O2Connect.Api.Models.RequestContexts;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services.TokenGrantHandlers;

public class ClientCredentialsGrantHandler : ITokenGrantHandler
{
    public ClientCredentialsGrantHandler()
    {
    }

    public GrantType GrantType => GrantType.ClientCredentials;

    public Task<TokenResponse> HandleAsync(TokenRequestContext context, CancellationToken ct)
    {
        var response = new TokenResponse
        {
            AccessToken = "mock_access_token",
            ExpiresIn = 3600,
            RefreshToken = "mock_refresh_token",
            IdToken = "mock_id_token"
        };

        return Task.FromResult(response);
    }
}
