using O2Connect.Api.Crypto.Validators;
using O2Connect.Api.DataFactories;
using O2Connect.Api.DataFactories.RequestModels;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.DataContexts;
using O2Connect.Api.Models.RequestInputs;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.DataHandlers.TokenGrantHandlers;

public class AuthorizationCodeGrantHandler : ITokenGrantHandler
{
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IPkceValidatorResolver _pkceResolver;
    private readonly ITokenFactory _tokenFactory;

    public GrantType GrantType => GrantType.AuthorizationCode;

    public AuthorizationCodeGrantHandler(
        IAuthorizationCodeRepository authorizationCodeRepository,
        IClientRepository clientRepository,
        IPkceValidatorResolver pkceResolver,
        ITokenFactory tokenFactory)
    {
        _authorizationCodeRepository = authorizationCodeRepository;
        _clientRepository = clientRepository;
        _pkceResolver = pkceResolver;
        _tokenFactory = tokenFactory;
    }

    public async Task<TokenResponse> HandleAsync(TokenRequestContext context, CancellationToken ct)
    {
        var input = context.Input;

        if (string.IsNullOrWhiteSpace(input.Code) ||
            string.IsNullOrWhiteSpace(input.RedirectUri))
            throw OAuthException.FromInvalidRequest();

        var code = await _authorizationCodeRepository.RedeemAsync(input.Code, ct)
            ?? throw OAuthException.FromInvalidGrant();

        await Validate(code, context, input, ct);

        return await _tokenFactory.GenerateAsync(new JwtTokenFactoryRequest
        {
            Client = context.Client,
            Scopes = ResolveScopes(context, code),
            Subject = code.SubjectId
        }, ct);
    }

    private async Task Validate(AuthorizationCode code, TokenRequestContext context, TokenInput input, CancellationToken ct)
    {
        if (code.ClientId != context.Client.ClientId)
            throw OAuthException.FromInvalidGrant();

        if (!context.Client.RequiresSecret && string.IsNullOrEmpty(code.CodeChallenge))
            throw OAuthException.FromInvalidGrant();

        if (code.ExpiresAt <= DateTime.UtcNow)
            throw OAuthException.FromInvalidGrant();

        if (!string.Equals(code.RedirectUri, input.RedirectUri, StringComparison.Ordinal))
            throw OAuthException.FromInvalidGrant();

        if (!await _clientRepository.ValidateRedirectUriAsync(context.Client.ClientId, input.RedirectUri!, ct))
            throw OAuthException.FromInvalidGrant();

        if (code.CodeChallenge != null)
        {
            if (string.IsNullOrWhiteSpace(code.CodeChallengeMethod))
                throw OAuthException.FromInvalidGrant();

            if (string.IsNullOrWhiteSpace(context.Input.CodeVerifier))
                throw OAuthException.FromInvalidGrant();

            var validator = _pkceResolver.Resolve(PkceMethod.Parse(code.CodeChallengeMethod));
            validator.Validate(context.Client, code, context.Input.CodeVerifier);
        }
    }

    private static IReadOnlyCollection<string> ResolveScopes(TokenRequestContext context, AuthorizationCode code)
    {
        if (context.RequestedScopes.IsEmpty)
            return code.Scopes.Values;

        if (!context.RequestedScopes.IsSubsetOf(code.Scopes.Values))
            throw OAuthException.FromInvalidGrant();

        return context.RequestedScopes.Values;
    }
}
