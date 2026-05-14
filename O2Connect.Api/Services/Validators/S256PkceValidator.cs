using System.Security.Cryptography;
using System.Text;

namespace O2Connect.Api.Services.Validators;

public class S256PkceValidator : IPkceValidator
{
    public string Method => "S256";

    public bool Validate(string verifier, string challenge)
    {
        using var sha256 = SHA256.Create();

        var bytes = Encoding.UTF8.GetBytes(verifier);
        var hash = sha256.ComputeHash(bytes);

        var computedChallenge = Convert.ToBase64String(hash)
                                       .Replace("+", "-")
                                       .Replace("/", "_")
                                       .Replace("=", "");

        if (string.Equals(computedChallenge, challenge, StringComparison.Ordinal))
            return true;

        return false;
    }
}
