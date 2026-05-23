using O2Connect.Api.Models;
using System.Security.Cryptography;
using System.Text;

namespace O2Connect.Api.Crypto.Validators;

public class PlainPkceValidator : IPkceValidator
{
    public PkceMethod Method => PkceMethod.Plain;

    public bool Validate(string verifier, string challenge)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(verifier),
            Encoding.UTF8.GetBytes(challenge));
    }
}
