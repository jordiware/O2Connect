using System.Security.Cryptography;
using System.Text;

namespace O2Connect.Api.Crypto.Validators;

public class S256PkceValidator : IPkceValidator
{
    public PkceMethod Method => PkceMethod.S256;

    public bool Validate(string verifier, string challenge)
    {
        using var sha256 = SHA256.Create();

        var bytes = Encoding.UTF8.GetBytes(verifier);
        var hash = sha256.ComputeHash(bytes);

        var computedChallenge = Convert.ToBase64String(hash)
                                       .Replace("+", "-")
                                       .Replace("/", "_")
                                       .Replace("=", "");

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedChallenge), 
            Encoding.UTF8.GetBytes(challenge));
    }
}
