using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Context;

namespace O2Connect.Api.DataValidators;

public interface ITokenInputValidator
{
    Task<TokenRequestContext> ValidateAsync(TokenRequestContext context, CancellationToken ct);
}

public class TokenInputValidator : ITokenInputValidator
{
    public async Task<TokenRequestContext> ValidateAsync(TokenRequestContext context, CancellationToken ct)
    {
        if (!context.Client.AllowedGrantTypes.Contains(context.GrantType.Value))
            throw OAuthException.FromUnauthorizedClient();

        if (context.Scopes is null || context.Scopes.Count == 0)
            throw OAuthException.FromInvalidScope();

        if (!context.Scopes.IsSubsetOf(context.Client.AllowedScopes))
            throw OAuthException.FromInvalidScope();

        return context;
    }
}
