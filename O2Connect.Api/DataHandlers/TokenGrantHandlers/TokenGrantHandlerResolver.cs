using O2Connect.Api.Exceptions;

namespace O2Connect.Api.DataHandlers.TokenGrantHandlers;

public interface ITokenGrantHandlerResolver
{
    ITokenGrantHandler Resolve(GrantType grantType);
}

public class TokenGrantHandlerResolver : ITokenGrantHandlerResolver
{
    private readonly Dictionary<string, ITokenGrantHandler> _handlers;

    public TokenGrantHandlerResolver(IEnumerable<ITokenGrantHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.GrantType.Value, StringComparer.Ordinal);
    }

    public ITokenGrantHandler Resolve(GrantType grantType)
    {
        if (!_handlers.TryGetValue(grantType.Value, out var handler))
            throw OAuthException.FromUnsupportedGrantType();

        return handler;
    }
}
