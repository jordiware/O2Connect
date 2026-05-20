using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.RequestContexts;
using O2Connect.Api.Models.RequestInputs;
using O2Connect.Api.Services.Authenticators;

namespace O2Connect.Api.Services.Validators;

public interface ITokenRequestValidator
{
    Task<TokenRequestContext> ValidateAsync(TokenInput input, CancellationToken ct);
}

public class TokenRequestValidator : ITokenRequestValidator
{
    private readonly IClientAuthenticator _clientAuth;
    private readonly IScopeValidator _scopeValidator;

    public TokenRequestValidator(
        IClientAuthenticator clientAuth,
        IScopeValidator scopeValidator)
    {
        _clientAuth = clientAuth;
        _scopeValidator = scopeValidator;
    }

    public async Task<TokenRequestContext> ValidateAsync(TokenInput input, CancellationToken ct)
    {
        var client = await _clientAuth.AuthenticateAsync(input, ct);

        if (!client.AllowedGrantTypes.Contains(input.GrantType.Value))
            throw OAuthException.FromUnauthorizedClient();

        if (input.Scopes == null)
            throw OAuthException.FromInvalidScope();

        var scopes = _scopeValidator.Validate(input.Scopes, client);

        return new TokenRequestContext(
            client,
            input.GrantType,
            scopes,
            input
        );
    }
}
