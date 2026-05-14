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

        if (!client.RedirectUris.Contains(request.RedirectUri!))
            throw new OAuthException("invalid_grant");

        if (string.IsNullOrWhiteSpace(request.RedirectUri))
            throw new OAuthException("invalid_request");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new OAuthException("invalid_request");

        var storedCode = await _authorizationCodeRepository.RedeemAsync(request.Code, ct);

        if (storedCode == null)
            throw new OAuthException("invalid_grant");

        if (storedCode.ClientId != client.ClientId)
            throw new OAuthException("invalid_grant");

        if (storedCode.ExpiresAt <= DateTime.UtcNow)
            throw new OAuthException("invalid_grant");

        if (!string.Equals(storedCode.RedirectUri, request.RedirectUri, StringComparison.Ordinal))
            throw new OAuthException("invalid_grant");

        if (validatedClient.RequestedScopes.Count > 0)
        {
            var requested = new HashSet<string>(validatedClient.RequestedScopes, StringComparer.Ordinal);
            var granted = new HashSet<string>(storedCode.Scopes ?? [], StringComparer.Ordinal);

            if (!requested.IsSubsetOf(granted))
                throw new OAuthException("invalid_grant");
        }

        if (storedCode.CodeChallenge != null)
        {
            if (string.IsNullOrWhiteSpace(storedCode.CodeChallengeMethod))
                throw new OAuthException("invalid_grant");

            if (string.IsNullOrWhiteSpace(request.CodeVerifier))
                throw new OAuthException("invalid_grant");

            if (!_pkceValidators.TryGetValue(storedCode.CodeChallengeMethod, out var pkceValidator))
                throw new OAuthException("invalid_grant");

            if (!pkceValidator.Validate(request.CodeVerifier, storedCode.CodeChallenge))
                throw new OAuthException("invalid_grant");
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
