using System.Security.Cryptography;

namespace O2Connect.Api.Crypto;

public interface ISecretHasher
{
    string Hash(string secret);
    bool Verify(string secret, string hashedSecret);
}

public class Pbkdf2SecretHasher : ISecretHasher
{
    private const int SaltSize = 16;   // 128-bit
    private const int KeySize = 32;    // 256-bit
    private const int Iterations = 100_000;

    public string Hash(string secret)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password: secret,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: KeySize);

        return Format(salt, hash);
    }

    public bool Verify(string secret, string hashedSecret)
    {
        if (string.IsNullOrWhiteSpace(hashedSecret))
            return false;

        if (!TryParse(hashedSecret, out var salt, out var expectedHash))
            return false;

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password: secret,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: KeySize);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static string Format(byte[] salt, byte[] hash)
    {
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool TryParse(string input, out byte[] salt, out byte[] hash)
    {
        salt = default!;
        hash = default!;

        var parts = input.Split('.', 2);
        if (parts.Length != 2)
            return false;

        try
        {
            salt = Convert.FromBase64String(parts[0]);
            hash = Convert.FromBase64String(parts[1]);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
