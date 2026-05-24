using O2Connect.Api.Models;
using System.Security.Cryptography;
using System.Text;

namespace O2Connect.Api.DataValidators.Crypto;

public class S256PkceValidator : PkceValidator, IPkceValidator
{
    public override PkceMethod Method => PkceMethod.S256;

    protected override bool Validate(string verifier, string challenge)
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
