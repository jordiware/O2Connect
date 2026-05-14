using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Repositories;
using O2Connect.Api.Services.Validators;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services.TokenGrantHandlers;

public class AuthorizationCodeGrantHandler : TokenGrantHandler
{
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;

    public AuthorizationCodeGrantHandler(
        IEnumerable<IPkceValidator> pkceValidators,
        IAuthorizationCodeRepository authorizationCodeRepository) : base(pkceValidators)
    {
        _authorizationCodeRepository = authorizationCodeRepository;
    }

    public override string GrantType => "authorization_code";

    public override async Task<TokenResponse> HandleAsync(TokenRequest request, ValidatedClient validatedClient, CancellationToken ct)
    {
        var client = validatedClient.Client;

        if (string.IsNullOrWhiteSpace(request.Code))
            throw OAuthException.FromInvalidRequest();

        if (string.IsNullOrWhiteSpace(request.RedirectUri))
            throw OAuthException.FromInvalidRequest();

        if (!client.RedirectUris.Contains(request.RedirectUri))
            throw OAuthException.FromInvalidGrant();

        var storedCode = await _authorizationCodeRepository.RedeemAsync(request.Code, ct);

        if (storedCode == null)
            throw OAuthException.FromInvalidGrant();

        if (storedCode.ClientId != client.ClientId)
            throw OAuthException.FromInvalidGrant();

        if (storedCode.ExpiresAt <= DateTime.UtcNow)
            throw OAuthException.FromInvalidGrant();

        if (!string.Equals(storedCode.RedirectUri, request.RedirectUri, StringComparison.Ordinal))
            throw OAuthException.FromInvalidGrant();

        if (validatedClient.RequestedScopes.Count > 0)
        {
            var requested = new HashSet<string>(validatedClient.RequestedScopes, StringComparer.Ordinal);
            var granted = new HashSet<string>(storedCode.Scopes ?? [], StringComparer.Ordinal);

            if (!requested.IsSubsetOf(granted))
                throw OAuthException.FromInvalidGrant();
        }
        else
        {
            validatedClient.RequestedScopes = storedCode.Scopes ?? [];
        }

        if (storedCode.CodeChallenge != null)
        {
            if (string.IsNullOrWhiteSpace(storedCode.CodeChallengeMethod))
                throw OAuthException.FromInvalidGrant();

            if (string.IsNullOrWhiteSpace(request.CodeVerifier))
                throw OAuthException.FromInvalidGrant();

            if (!_pkceValidators.TryGetValue(storedCode.CodeChallengeMethod, out var pkceValidator))
                throw OAuthException.FromInvalidGrant();

            if (!pkceValidator.Validate(request.CodeVerifier, storedCode.CodeChallenge))
                throw OAuthException.FromInvalidGrant();
        }

        return new TokenResponse
        {
            AccessToken = "mock_access_token",
            ExpiresIn = 3600,
            RefreshToken = "mock_refresh_token",
            IdToken = "mock_id_token"
        };
    }
}
