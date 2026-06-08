using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace O2Connect.Api.Crypto;

public static class SecureCodeGenerator
{
    public static byte[] GenerateTokenBytes(int length = 32)
    {
        const int MaxAllowedLength = 4096;
        const int MaxStackAlloc = 256;

        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (length > MaxAllowedLength)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (length <= MaxStackAlloc)
        {
            Span<byte> bytes = stackalloc byte[length];
            RandomNumberGenerator.Fill(bytes);
            return bytes.ToArray();
        }
        else
        {
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            return bytes;
        }
    }

    public static string GenerateBase64UrlToken(int length = 32, string prefix = "")
    {
        var bytes = GenerateTokenBytes(length);
        return $"{prefix}{Base64UrlEncoder.Encode(bytes)}";
    }
}
