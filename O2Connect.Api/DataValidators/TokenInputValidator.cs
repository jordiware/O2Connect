using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Context;

namespace O2Connect.Api.DataValidators;

public interface ITokenInputValidator
{
    Task<TokenRequestContext> ValidateAsync(TokenRequestContext context, CancellationToken ct);
}

public class TokenInputValidator : ITokenInputValidator
{
    private readonly IScopeValidator _scopeValidator;

    public TokenInputValidator(
        IScopeValidator scopeValidator)
    {
        _scopeValidator = scopeValidator;
    }

    public async Task<TokenRequestContext> ValidateAsync(TokenRequestContext context, CancellationToken ct)
    {
        if (!context.Client.AllowedGrantTypes.Contains(context.GrantType.Value))
            throw OAuthException.FromUnauthorizedClient();

        var scopes = _scopeValidator.Validate(context.Scopes, context.Client);

        return context;
    }
}
