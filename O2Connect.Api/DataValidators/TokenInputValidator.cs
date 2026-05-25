using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.DataContexts;
using O2Connect.Api.Models.RequestInputs;
using O2Connect.Api.Services.Authenticators;

namespace O2Connect.Api.DataValidators;

public interface ITokenInputValidator
{
    Task<TokenRequestContext> ValidateAsync(TokenRequestInput input, CancellationToken ct);
}

public class TokenInputValidator : ITokenInputValidator
{
    private readonly IClientAuthenticator _clientAuth;
    private readonly IScopeValidator _scopeValidator;

    public TokenInputValidator(
        IClientAuthenticator clientAuth,
        IScopeValidator scopeValidator)
    {
        _clientAuth = clientAuth;
        _scopeValidator = scopeValidator;
    }

    public async Task<TokenRequestContext> ValidateAsync(TokenRequestInput input, CancellationToken ct)
    {
        var client = await _clientAuth.AuthenticateAsync(input, ct);

        if (!client.AllowedGrantTypes.Contains(input.GrantType.Value))
            throw OAuthException.FromUnauthorizedClient();

        var scopes = _scopeValidator.Validate(input.Scopes, client);

        return new TokenRequestContext(
            client,
            input.GrantType,
            scopes,
            input
        );
    }
}
