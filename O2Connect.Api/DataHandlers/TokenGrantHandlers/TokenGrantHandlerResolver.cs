using O2Connect.Api.Models;
using System.Diagnostics.CodeAnalysis;

namespace O2Connect.Api.DataHandlers.TokenGrantHandlers;

public interface ITokenGrantHandlerResolver
{
    bool TryResolve(GrantType grantType, [NotNullWhen(true)] out ITokenGrantHandler handler);
}

public class TokenGrantHandlerResolver : ITokenGrantHandlerResolver
{
    private readonly Dictionary<GrantType, ITokenGrantHandler> _handlers;

    public TokenGrantHandlerResolver(IEnumerable<ITokenGrantHandler> handlers)
    {
        var dict = new Dictionary<GrantType, ITokenGrantHandler>();

        foreach (var handler in handlers)
        {
            if (!dict.TryAdd(handler.GrantType, handler))
                throw new InvalidOperationException($"Duplicate grant type validator registered: {handler.GrantType}");
        }

        _handlers = dict;
    }

    public bool TryResolve(GrantType grantType, [NotNullWhen(true)] out ITokenGrantHandler handler)
    {
        return _handlers.TryGetValue(grantType, out handler!);
    }
}
