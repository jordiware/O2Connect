using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.Security.Cryptography;
using System.Text;

namespace O2Connect.Api.Services;

public interface ITokenService
{
    Task<TokenResponse> HandleAsync(TokenRequest request);
}

public class TokenService : ITokenService
{
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
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
        var client = await _clientRepository.GetByIdAsync(request.ClientId!);

        if (client == null)
            throw new Exception("invalid_client");

        if (client.RequiresSecret)
        {
            if (!await _clientRepository.ValidateClientAsync(request.ClientId!, request.ClientSecret))
                throw new Exception("invalid_client");
        }

        if (string.IsNullOrWhiteSpace(request.GrantType))
        {
            throw new Exception("invalid_request");
        }

        switch (request.GrantType)
        {
            case "authorization_code":
                var storedCode = await _authorizationCodeRepository.GetAsync(request.Code!);

                if (storedCode == null)
                {
                    throw new Exception("invalid_grant");
                }

                if (storedCode.ClientId != request.ClientId)
                {
                    throw new Exception("invalid_grant");
                }

                if (storedCode.ExpiresAt < DateTime.UtcNow)
                {
                    throw new Exception("invalid_grant");
                }

                if (string.IsNullOrWhiteSpace(request.RedirectUri))
                {
                    throw new Exception("invalid_request");
                }

                if (storedCode.RedirectUri != request.RedirectUri)
                {
                    throw new Exception("invalid_grant");
                }

                if (string.IsNullOrWhiteSpace(request.CodeVerifier))
                {
                    throw new Exception("invalid_request");
                }

                if (!string.Equals(storedCode.CodeChallengeMethod, "S256", StringComparison.Ordinal))
                {
                    throw new Exception("invalid_grant");
                }

                using (var sha256 = SHA256.Create())
                {
                    var hashed = sha256.ComputeHash(Encoding.ASCII.GetBytes(request.CodeVerifier));
                    var computedChallenge = Convert.ToBase64String(hashed);

                    if (!string.Equals(computedChallenge, storedCode.CodeChallenge, StringComparison.Ordinal))
                    {
                        throw new Exception("invalid_grant");
                    }
                }

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
