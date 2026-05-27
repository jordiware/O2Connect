using O2Connect.Api.Models;
using O2Connect.Api.Models.Context;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.DataHandlers.TokenGrantHandlers;

public class RefreshTokenGrantHandler : ITokenGrantHandler
{
    public RefreshTokenGrantHandler()
    {
    }

    public GrantType GrantType => GrantType.RefreshToken;

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
