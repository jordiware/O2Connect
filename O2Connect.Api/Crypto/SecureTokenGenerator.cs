using System.Security.Cryptography;

namespace O2Connect.Api.Crypto;

public interface ISecureTokenGenerator
{
    string GenerateSecureToken(int numBytes = 64);
}

public class SecureTokenGenerator : ISecureTokenGenerator
{
    public string GenerateSecureToken(int numBytes = 64)
    {
        Span<byte> bytes = numBytes <= 256
            ? stackalloc byte[numBytes]
            : new byte[numBytes];

        RandomNumberGenerator.Fill(bytes);

        var token = Convert.ToBase64String(bytes)
                           .Replace("+", "-")
                           .Replace("/", "_")
                           .TrimEnd('=');

        return $"rt_{token}";
    }
}
