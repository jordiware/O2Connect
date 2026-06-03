using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace O2Connect.Api.Crypto;

public interface ISecretHasher
{
    bool TryHash(string secret, [NotNullWhen(true)] out string? hashedSecret);
    bool NeedsRehash(string hashedSecret);
    bool Verify(string secret, string hashedSecret);
}

public class Pbkdf2SecretHasher : ISecretHasher
{
    private const string Scheme = "pbkdf2";
    private const string Algo = "sha256";

    private const int SaltSize = 16;   // 128-bit
    private const int KeySize = 32;    // 256-bit
    private const int Iterations = 100_000;

    private const int MinSaltSize = 12;   // 96-bit
    private const int MinKeySize = 24;    // 192-bit
    private const int MinIterations = 10_000;

    private const int MaxSaltSize = 32;   // 256-bit
    private const int MaxKeySize = 64;    // 512-bit
    private const int MaxIterations = 1_000_000;

    public bool TryHash(string secret, [NotNullWhen(true)] out string? hashedSecret)
    {
        hashedSecret = null;

        if (string.IsNullOrWhiteSpace(secret))
            return false;

        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password: secret,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: KeySize);

        hashedSecret = Format(salt, hash);

        return true;
    }

    public bool NeedsRehash(string hashedSecret)
    {
        if (!TryParse(hashedSecret, out var iterations, out var salt, out var hash))
            return true;

        return iterations < Iterations
               || salt.Length < SaltSize
               || hash.Length < KeySize;
    }

    public bool Verify(string secret, string hashedSecret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            return false;

        if (string.IsNullOrWhiteSpace(hashedSecret))
            return false;

        if (!TryParse(hashedSecret, out var iterations, out var salt, out var expectedHash))
            return false;

        if (iterations is < MinIterations or > MaxIterations)
            return false;

        if (salt.Length is < MinSaltSize or > MaxSaltSize)
            return false;

        if (expectedHash.Length is < MinKeySize or > MaxKeySize)
            return false;

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password: secret,
            salt: salt,
            iterations: iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static string Format(byte[] salt, byte[] hash)
    {
        return string.Join('$', Scheme, Algo, Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    private static bool TryParse(string input, out int iterations, out byte[] salt, out byte[] hash)
    {
        iterations = default;
        salt = default!;
        hash = default!;

        var parts = input.Trim().Split('$', 5);
        if (parts.Length != 5)
            return false;

        if (!string.Equals(parts[0], Scheme, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.Equals(parts[1], Algo, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!int.TryParse(parts[2], out iterations))
            return false;

        try
        {
            salt = Convert.FromBase64String(parts[3]);
            hash = Convert.FromBase64String(parts[4]);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
