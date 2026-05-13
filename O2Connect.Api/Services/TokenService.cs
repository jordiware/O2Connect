using O2Connect.Api.Exceptions;
using O2Connect.Api.Repositories;
using O2Connect.Api.Services.Helpers;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.Security.Cryptography;
using System.Text;

namespace O2Connect.Api.Services;

public interface ITokenService
{
    Task<TokenResponse> HandleAsync(TokenRequest request, CancellationToken ct);
}

public class TokenService : ITokenService
{
    private readonly IPkceHelper _pkceHelper;
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IClientRepository _clientRepository;

    public TokenService(
        IPkceHelper pkceHelper,
        IAuthorizationCodeRepository authorizationCodeRepository,
        IClientRepository clientRepository)
    {
        _pkceHelper = pkceHelper;
        _authorizationCodeRepository = authorizationCodeRepository;
        _clientRepository = clientRepository;
    }

    public async Task<TokenResponse> HandleAsync(TokenRequest request, CancellationToken ct)
    {
        var client = await _clientRepository.GetByIdAsync(request.ClientId);

        if (client == null)
        {
            throw new OAuthException("invalid_client", "No client found for given client_id");
        }

        if (client.RequiresSecret
            && await _clientRepository.ValidateClientAsync(request.ClientId, request.ClientSecret))
        {
            throw new OAuthException("invalid_client");
        }

        return request.GrantType switch
        {
            "authorization_code" => await HandleAuthorizationCodeGrantTypeAsync(request, ct),
            "client_credentials" => await HandleClientCredentialsGrantTypeAsync(request, ct),
            "refresh_token" => await HandleRefreshTokenGrantTypeAsync(request, ct),
            _ => throw new OAuthException("unsupported_grant_type")
        };
    }

    public async Task<TokenResponse> HandleAuthorizationCodeGrantTypeAsync(TokenRequest request, CancellationToken ct)
    {
        var storedCode = await _authorizationCodeRepository.RedeemAsync(request.Code!);

        if (storedCode == null)
            throw new OAuthException("invalid_grant");

        if (storedCode.ClientId != request.ClientId)
            throw new OAuthException("invalid_grant");

        if (storedCode.ExpiresAt < DateTime.UtcNow)
            throw new OAuthException("invalid_grant");

        if (storedCode.RedirectUri != request.RedirectUri)
            throw new OAuthException("invalid_grant");

        _pkceHelper.Validate(request.CodeVerifier!, storedCode.CodeChallenge!, storedCode.CodeChallengeMethod!);

        var response = new TokenResponse
        {
            AccessToken = "mock_access_token",
            ExpiresIn = 3600,
            RefreshToken = "mock_refresh_token",
            IdToken = "mock_id_token"
        };

        return response;
    }

    public async Task<TokenResponse> HandleClientCredentialsGrantTypeAsync(TokenRequest request, CancellationToken ct)
    {
        var response = new TokenResponse
        {
            AccessToken = "mock_access_token",
            ExpiresIn = 3600,
            RefreshToken = "mock_refresh_token",
            IdToken = "mock_id_token"
        };

        return response;
    }

    public async Task<TokenResponse> HandleRefreshTokenGrantTypeAsync(TokenRequest request, CancellationToken ct)
    {
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
