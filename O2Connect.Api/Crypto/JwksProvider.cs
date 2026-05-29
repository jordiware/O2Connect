using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Exceptions;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace O2Connect.Api.Crypto;

public interface IJwksProvider
{
    Task<IEnumerable<SecurityKey>> GetKeysAsync(string jwksUri,
                                                string? kid,
                                                string expectedAlg,
                                                CancellationToken ct);

    void Invalidate(string jwksUri);
}

public class JwksProvider : IJwksProvider
{
    const long MaxJwksSize = 256 * 1024; // 256 KB

    private readonly IMemoryCache _cache;

    public JwksProvider(
        IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<IEnumerable<SecurityKey>> GetKeysAsync(string jwksUri,
                                                             string? kid,
                                                             string expectedAlg,
                                                             CancellationToken ct)
    {
        var jwks = await GetOrFetchAsync(jwksUri, ct);

        if (jwks == null)
            throw OAuthException.FromInvalidClient();

        var keys = jwks.Keys.Where(k => k.Kty == "RSA")
                            .Where(k => k.Use == null || k.Use == "sig")
                            .Where(k => k.KeyOps == null || k.KeyOps.Contains("verify"))
                            .Where(k => k.Alg == null || k.Alg == expectedAlg)
                            .Where(k => k.N != null && GetKeySize(k) is >= 2048 and <= 8192)
                            .ToList();

        if (!string.IsNullOrEmpty(kid))
        {
            keys = keys.Where(k => k.Kid == kid).ToList();

            if (keys.Count != 1)
                throw OAuthException.FromInvalidClient();
        }
        else
        {
            if (keys.Count != 1)
                throw OAuthException.FromInvalidClient();
        }

        return keys;
    }

    public void Invalidate(string jwksUri)
    {
        var normalized = new Uri(jwksUri).GetLeftPart(UriPartial.Path);
        _cache.Remove(normalized);
    }

    private async Task<JsonWebKeySet?> GetOrFetchAsync(string uri, CancellationToken ct)
    {
        var parsed = new Uri(uri);

        if (parsed.Scheme != Uri.UriSchemeHttps)
            throw OAuthException.FromInvalidClient();

        if (parsed.Port != 443 && parsed.Port != -1)
            throw OAuthException.FromInvalidClient();

        var normalized = parsed.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.UriEscaped);

        var addresses = await Dns.GetHostAddressesAsync(parsed.Host, ct);

        if (addresses == null || addresses.Length == 0)
            throw OAuthException.FromInvalidClient();

        var publicAddresses = addresses.Where(a => !IsPrivateAddress(a)).ToList();

        if (publicAddresses.Count != 1)
            throw OAuthException.FromInvalidClient();

        if (_cache.TryGetValue<JsonWebKeySet>(normalized, out var cached))
            return cached;

        var address = publicAddresses[0];

        var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            ConnectCallback = async (context, cancellationToken) =>
            {
                var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                await socket.ConnectAsync(new IPEndPoint(address, context.DnsEndPoint.Port),
                                          cancellationToken);

                return new NetworkStream(socket, ownsSocket: true);
            },
            SslOptions = new SslClientAuthenticationOptions
            {
                TargetHost = parsed.Host,
                RemoteCertificateValidationCallback = (sender, cert, chain, errors) =>
                {
                    return errors == SslPolicyErrors.None;
                }
            }
        };

        using var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
            throw OAuthException.FromInvalidClient();

        if (response.Content.Headers.ContentType?.MediaType != "application/json")
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

        var json = Encoding.UTF8.GetString(limitedStream.ToArray());

        var jwks = new JsonWebKeySet(json);

        if (jwks.Keys == null || jwks.Keys.Count == 0 || jwks.Keys.Count > 10)
            throw OAuthException.FromInvalidClient();

        _cache.Set(normalized, jwks, TimeSpan.FromMinutes(10));

        return jwks;
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
