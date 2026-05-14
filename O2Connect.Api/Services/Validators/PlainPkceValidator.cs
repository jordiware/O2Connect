using System.Security.Cryptography;
using System.Text;

namespace O2Connect.Api.Services.Validators;

public class PlainPkceValidator : IPkceValidator
{
    public string Method => "plain";

    public bool Validate(string verifier, string challenge)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(verifier),
            Encoding.UTF8.GetBytes(challenge));
    }
}
