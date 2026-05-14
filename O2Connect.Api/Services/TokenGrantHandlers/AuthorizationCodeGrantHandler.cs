using O2Connect.Api.Exceptions;
using O2Connect.Api.Repositories;
using O2Connect.Api.Services.PkceValidators;
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

    public override async Task<TokenResponse> HandleAsync(TokenRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RedirectUri))
            throw new OAuthException("invalid_request");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new OAuthException("invalid_request");

        if (string.IsNullOrWhiteSpace(request.CodeVerifier))
            throw new OAuthException("invalid_request");

        var storedCode = await _authorizationCodeRepository.RedeemAsync(request.Code, ct);

        if (storedCode == null)
            throw new OAuthException("invalid_grant");

        if (storedCode.ClientId != request.ClientId)
            throw new OAuthException("invalid_grant");

        if (storedCode.ExpiresAt < DateTime.UtcNow)
            throw new OAuthException("invalid_grant");

        if (storedCode.RedirectUri != request.RedirectUri)
            throw new OAuthException("invalid_grant");

        var pkceValidator = _pkceValidators.FirstOrDefault(v => v.Method == storedCode.CodeChallengeMethod);

        if (pkceValidator == null)
            throw new OAuthException("invalid_grant");

        pkceValidator.Validate(request.CodeVerifier, storedCode.CodeChallenge!);

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
