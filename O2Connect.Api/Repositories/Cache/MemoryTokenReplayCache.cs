using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace O2Connect.Api.Repositories.Cache;

public class MemoryTokenReplayCache : ITokenReplayCache
{
    private readonly IMemoryCache _cache;

    public MemoryTokenReplayCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool TryAdd(string securityToken, DateTime expiresOn)
    {
        var added = _cache.GetOrCreate(securityToken, entry =>
        {
            entry.AbsoluteExpiration = expiresOn;
            return true;
        });

        return added == true;
    }

    public bool TryFind(string securityToken)
    {
        return _cache.TryGetValue(securityToken, out _);
    }
}
