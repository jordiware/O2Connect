using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services;

public interface ITokenService
{
    Task<TokenResponse> HandleAsync(TokenRequest request);
}

public class TokenService : ITokenService
{
    IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IClientRepository _clientRepository;

    public TokenService(
        IAuthorizationCodeRepository authorizationCodeRepository,
        IClientRepository clientRepository)
    {
        _authorizationCodeRepository = authorizationCodeRepository;
        _clientRepository = clientRepository;
    }

    public async Task<TokenResponse> HandleAsync(TokenRequest request)
    {
        var isValidClient = await _clientRepository
            .ValidateClientAsync(request.ClientId!, request.ClientSecret);

        if (!isValidClient)
        {
            throw new Exception("invalid_client");
        }

        switch (request.GrantType)
        {
            case "authorization_code":
                var storedCode = await _authorizationCodeRepository.GetAsync(request.Code!);

                if (storedCode == null)
                {
                    throw new Exception("invalid_grant");
                }

                if (storedCode.ExpiresAt < DateTime.UtcNow)
                {
                    throw new Exception("invalid_grant");
                }

                if (storedCode.RedirectUri != request.RedirectUri)
                {
                    throw new Exception("invalid_grant");
                }

                // (Later) PKCE validation goes here

                await _authorizationCodeRepository.RemoveAsync(request.Code!);

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

        return response;
    }
}
