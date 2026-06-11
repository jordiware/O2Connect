using O2Connect.Api.DataHandlers.TokenContextHandlers;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Context;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services;

public interface ITokenService
{
    Task<TokenResponse> HandleAsync(TokenRequestContext context, CancellationToken ct);
}

public class TokenService : ITokenService
{
    private readonly ITokenContextHandlerResolver _grantResolver;

    public TokenService(
        ITokenContextHandlerResolver grantResolver)
    {
        _grantResolver = grantResolver;
    }

    public Task<TokenResponse> HandleAsync(TokenRequestContext context, CancellationToken ct)
    {
        if (!_grantResolver.TryResolve(context.GrantType, out var handler))
            throw OAuthException.FromUnsupportedGrantType();

        return handler.HandleAsync(context, ct);
    }
}
