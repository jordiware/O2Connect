using O2Connect.Api.Models.SmartEnums;
using System.Diagnostics.CodeAnalysis;

namespace O2Connect.Api.DataHandlers.TokenContextHandlers;

public interface ITokenContextHandlerResolver
{
    bool TryResolve(GrantType grantType, [NotNullWhen(true)] out ITokenContextHandler handler);
}

public class TokenContextHandlerResolver : ITokenContextHandlerResolver
{
    private readonly Dictionary<GrantType, ITokenContextHandler> _handlers;

    public TokenContextHandlerResolver(IEnumerable<ITokenContextHandler> handlers)
    {
        var dict = new Dictionary<GrantType, ITokenContextHandler>();

        foreach (var handler in handlers)
        {
            if (!dict.TryAdd(handler.GrantType, handler))
                throw new InvalidOperationException($"Duplicate grant type validator registered: {handler.GrantType}");
        }

        _handlers = dict;
    }

    public bool TryResolve(GrantType grantType, [NotNullWhen(true)] out ITokenContextHandler handler)
    {
        return _handlers.TryGetValue(grantType, out handler!);
    }
}
