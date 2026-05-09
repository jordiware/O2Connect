using O2Connect.Api.RequestDtos;
using O2Connect.Api.ResponseDtos;

namespace O2Connect.Api.Services;

public interface ITokenService
{
    Task<TokenResponse> HandleAsync(TokenRequest request);
}

public class TokenService : ITokenService
{
    public Task<TokenResponse> HandleAsync(TokenRequest request)
    {
        switch (request.GrantType)
        {
            case "authorization_code":
                // handle code flow
                break;

            case "client_credentials":
                // handle machine-to-machine
                break;

            case "refresh_token":
                // handle refresh
                break;

            default:
                throw new Exception("unsupported_grant_type");
        }

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
