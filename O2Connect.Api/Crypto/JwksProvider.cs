using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Exceptions;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace O2Connect.Api.Crypto;

public interface IJwksProvider
{
    Task<IReadOnlyList<SecurityKey>> GetKeysAsync(string jwksUri,
                                                  string? kid,
                                                  string expectedAlg,
                                                  CancellationToken ct);

    void Invalidate(string jwksUri);
}

public class JwksProvider : IJwksProvider
{
    const long MaxJwksSize = 256 * 1024; // 256 KB

    private readonly IMemoryCache _cache;
    private readonly ILogger<JwksProvider> _logger;

    public JwksProvider(
        IMemoryCache cache,
        ILogger<JwksProvider> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SecurityKey>> GetKeysAsync(string jwksUri,
                                                               string? kid,
                                                               string expectedAlg,
                                                               CancellationToken ct)
    {
        var jwks = await GetOrFetchAsync(jwksUri, ct);

        if (jwks == null)
            throw OAuthException.FromInvalidClient();

        var keys = jwks.Keys.Where(k => FilterKeys(k, expectedAlg)).ToList();

        if (!string.IsNullOrEmpty(kid))
        {
            keys = keys.Where(k => k.Kid == kid).ToList();

            if (keys.Count != 1)
                throw OAuthException.FromInvalidClient();
        }
        else if (keys.Count == 0)
        {
            throw OAuthException.FromInvalidClient();
        }

        return keys;
    }

    public void Invalidate(string jwksUri)
    {
        var parsed = new Uri(jwksUri);
        var normalized = parsed.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path | UriComponents.Query, UriFormat.UriEscaped);
        var jwksCacheKey = $"jwks:{normalized}";
        _cache.Remove(jwksCacheKey);
    }

    private async Task<JsonWebKeySet?> GetOrFetchAsync(string uri, CancellationToken ct)
    {
        var parsed = new Uri(uri);

        if (parsed.Scheme != Uri.UriSchemeHttps)
            throw OAuthException.FromInvalidClient();

        if (parsed.Port != 443 && parsed.Port != -1)
            throw OAuthException.FromInvalidClient();

        var normalized = parsed.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path | UriComponents.Query, UriFormat.UriEscaped);

        var jwksCacheKey = $"jwks:{normalized}";
        if (_cache.TryGetValue<JsonWebKeySet>(jwksCacheKey, out var cached))
            return cached;

        var publicAddresses = (await Dns.GetHostAddressesAsync(parsed.Host, ct))
                                        .Where(a => !IsPrivateAddress(a))
                                        .ToList();

        if (publicAddresses.Count != 1)
            throw OAuthException.FromInvalidClient();

        var address = publicAddresses[0];

        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Options.Set(new HttpRequestOptionsKey<IPAddress>("ResolvedIP"), address);

        var port = parsed.Port == -1 ? 443 : parsed.Port;
        var key = $"{parsed.Scheme}://{parsed.Host}:{port}";

        var handler = GetOrCreateHandler(key, parsed.Host);

        using var httpClient = new HttpClient(handler, disposeHandler: false)
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
            throw OAuthException.FromInvalidClient();

        if (response.Content.Headers.ContentType?.MediaType == null)
            throw OAuthException.FromInvalidClient();

        if (response.Content.Headers.ContentType?.MediaType.StartsWith("application/json") == false)
            throw OAuthException.FromInvalidClient();

        if (response.Content.Headers.ContentLength is long len && len > MaxJwksSize)
            throw OAuthException.FromInvalidClient();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);

        using var limitedStream = new MemoryStream();
        var buffer = new byte[8192];
        long totalRead = 0;

        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
        {
            totalRead += read;

            if (totalRead > MaxJwksSize)
                throw OAuthException.FromInvalidClient();

            limitedStream.Write(buffer, 0, read);
        }

        limitedStream.Position = 0;
        using var reader = new StreamReader(limitedStream, Encoding.UTF8);
        var json = await reader.ReadToEndAsync(ct);

        try
        {
            var jwks = new JsonWebKeySet(json);

            if (jwks.Keys == null || jwks.Keys.Count == 0 || jwks.Keys.Count > 10)
                throw OAuthException.FromInvalidClient();

            _cache.Set(jwksCacheKey, jwks, TimeSpan.FromMinutes(10));

            return jwks;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid JWKS from {Uri}", uri);
            throw OAuthException.FromInvalidClient();
        }
    }

    private SocketsHttpHandler GetOrCreateHandler(string key, string host)
    {
        var handlerCacheKey = $"handler:{key}";

        if (_cache.TryGetValue<SocketsHttpHandler>(handlerCacheKey, out var existing))
            return existing ?? throw new KeyNotFoundException();

        var handler = CreateHandler(host);

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(5),
            Size = 1 // optional, if you enable size limits
        };

        options.RegisterPostEvictionCallback((k, value, reason, state) =>
        {
            if (value is SocketsHttpHandler h)
            {
                try
                {
                    h.Dispose();
                }
                catch
                {
                    // swallow — disposal shouldn't crash anything
                }
            }
        });

        _cache.Set(handlerCacheKey, handler, options);

        return handler;
    }

    private SocketsHttpHandler CreateHandler(string host)
    {
        return new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false,

            // Keep connections alive
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 10,

            SslOptions = new SslClientAuthenticationOptions
            {
                TargetHost = host,
                CertificateRevocationCheckMode = X509RevocationMode.Online
            },
            ConnectCallback = async (context, ct) =>
            {
                var containsAddress = context.InitialRequestMessage.Options.TryGetValue(new HttpRequestOptionsKey<IPAddress>("ResolvedIP"), out IPAddress? address);

                if (!containsAddress || address == null)
                    throw new IOException("Missing resolved IP");

                var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                await socket.ConnectAsync(
                    new IPEndPoint(address, context.DnsEndPoint.Port),
                    ct);

                return new NetworkStream(socket, ownsSocket: true);
            },

        };
    }

    private static bool FilterKeys(JsonWebKey key, string expectedAlg)
    {
        if (key.Kty != "RSA")
            return false;

        if (key.Use != null && key.Use != "sig")
            return false;

        if (key.KeyOps != null && !key.KeyOps.Contains("verify"))
            return false;

        if (key.Alg != null && key.Alg != expectedAlg)
            return false;

        var size = GetKeySize(key);
        if (key.N == null || size < 2048 || size > 4096)
            return false;

        return true;
    }

    private static bool IsPrivateAddress(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip))
            return true;

        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = ip.GetAddressBytes();

            return bytes[0] switch
            {
                10 => true,
                172 when bytes[1] >= 16 && bytes[1] <= 31 => true,
                192 when bytes[1] == 168 => true,
                169 when bytes[1] == 254 => true,
                _ => false
            };
        }

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return ip.IsIPv6LinkLocal ||
                   ip.IsIPv6SiteLocal ||
                   ip.IsIPv6UniqueLocal;
        }

        return false;
    }

    private static int GetKeySize(JsonWebKey key)
    {
        if (string.IsNullOrEmpty(key.N))
            return 0;

        var modulusBytes = Base64UrlEncoder.DecodeBytes(key.N);

        if (modulusBytes.Length > 0 && modulusBytes[0] == 0x00)
        {
            var trimmed = new byte[modulusBytes.Length - 1];
            Buffer.BlockCopy(modulusBytes, 1, trimmed, 0, trimmed.Length);
            modulusBytes = trimmed;
        }

        return modulusBytes.Length * 8;
    }
}
