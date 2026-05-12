using O2Connect.Api.Exceptions;
using O2Connect.Api.Repositories;
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
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IClientRepository _clientRepository;

    public TokenService(
        IAuthorizationCodeRepository authorizationCodeRepository,
        IClientRepository clientRepository)
    {
        _authorizationCodeRepository = authorizationCodeRepository;
        _clientRepository = clientRepository;
    }

    public async Task<TokenResponse> HandleAsync(TokenRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId))
        {
            throw new OAuthException("invalid_request");
        }

        var client = await _clientRepository.GetByIdAsync(request.ClientId);

        if (client == null)
        {
            throw new OAuthException("invalid_client");
        }

        if (client.RequiresSecret
            && await _clientRepository.ValidateClientAsync(request.ClientId, request.ClientSecret))
        {
            throw new OAuthException("invalid_client");
        }

        if (string.IsNullOrWhiteSpace(request.GrantType))
        {
            throw new OAuthException("invalid_request");
        }

        return request.GrantType switch
        {
            GrantTypes.AuthorizationCode => await HandleAuthorizationCodeGrantTypeAsync(request, ct),
            GrantTypes.ClientCredentials => await HandleClientCredentialsGrantTypeAsync(request, ct),
            GrantTypes.RefreshToken => await HandleRefreshTokenGrantTypeAsync(request, ct),
            _ => throw new OAuthException("unsupported_grant_type")
        };
    }

    public async Task<TokenResponse> HandleAuthorizationCodeGrantTypeAsync(TokenRequest request, CancellationToken ct)
    {
        var storedCode = await _authorizationCodeRepository.GetAsync(request.Code!);

        if (storedCode == null)
        {
            throw new OAuthException("invalid_grant");
        }

        if (storedCode.ClientId != request.ClientId)
        {
            throw new OAuthException("invalid_grant");
        }

        if (storedCode.ExpiresAt < DateTime.UtcNow)
        {
            throw new OAuthException("invalid_grant");
        }

        if (string.IsNullOrWhiteSpace(request.RedirectUri))
        {
            throw new OAuthException("invalid_request");
        }

        if (storedCode.RedirectUri != request.RedirectUri)
        {
            throw new OAuthException("invalid_grant");
        }

        if (string.IsNullOrWhiteSpace(request.CodeVerifier))
        {
            throw new OAuthException("invalid_request");
        }

        if (!string.Equals(storedCode.CodeChallengeMethod, "S256", StringComparison.Ordinal))
        {
            throw new OAuthException("invalid_grant");
        }

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(request.CodeVerifier);
        var hash = sha256.ComputeHash(bytes);
        var computedChallenge = Convert.ToBase64String(hash)
                                       .Replace("+", "-")
                                       .Replace("/", "_")
                                       .Replace("=", "");

        if (!string.Equals(computedChallenge, storedCode.CodeChallenge, StringComparison.Ordinal))
        {
            throw new OAuthException("invalid_grant");
        }

        await _authorizationCodeRepository.RemoveAsync(request.Code!);

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

    public static class GrantTypes
    {
        public const string AuthorizationCode = "authorization_code";
        public const string ClientCredentials = "client_credentials";
        public const string RefreshToken = "refresh_token";
    }
}
