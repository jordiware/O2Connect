using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Exceptions;
using System.Net;

namespace O2Connect.Api.Crypto;

public interface IJwksProvider
{
    Task<IEnumerable<SecurityKey>> GetKeysAsync(string jwksUri, string? kid, CancellationToken ct);
    void Invalidate(string jwksUri);
}

public class JwksProvider : IJwksProvider
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    public JwksProvider(
        HttpClient httpClient,
        IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<IEnumerable<SecurityKey>> GetKeysAsync(string jwksUri, string? kid, CancellationToken ct)
    {
        var jwks = await GetOrFetchAsync(jwksUri, ct);

        if (jwks == null)
            throw OAuthException.FromInvalidClient();

        var keys = jwks.Keys.Where(k => k.Kty == "RSA").ToList();

        if (!string.IsNullOrEmpty(kid))
            keys = keys.Where(k => k.Kid == kid).ToList();

        if (!keys.Any())
            throw OAuthException.FromInvalidClient();

        return keys;
    }

    public void Invalidate(string jwksUri)
    {
        _cache.Remove(jwksUri);
    }

    private async Task<JsonWebKeySet?> GetOrFetchAsync(string uri, CancellationToken ct)
    {
        var parsed = new Uri(uri);

        if (parsed.Scheme != Uri.UriSchemeHttps)
            throw OAuthException.FromInvalidClient();

        if (IsPrivateAddress(parsed.Host))
            throw OAuthException.FromInvalidClient();

        if (_cache.TryGetValue<JsonWebKeySet>(uri, out var cached))
            return cached;

        var response = await _httpClient.GetAsync(uri, ct);

        if (!response.IsSuccessStatusCode)
            throw OAuthException.FromInvalidClient();

        var json = await response.Content.ReadAsStringAsync(ct);

        var jwks = new JsonWebKeySet(json);

        _cache.Set(uri, jwks, TimeSpan.FromMinutes(10));

        return jwks;
    }

    private bool IsPrivateAddress(string host)
    {
        if (IPAddress.TryParse(host, out var ip))
        {
            return IPAddress.IsLoopback(ip) ||
                   ip.IsIPv6LinkLocal ||
                   ip.IsIPv6SiteLocal;
        }

        return false;
    }
}
