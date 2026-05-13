using O2Connect.Api.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace O2Connect.Api.Services.Helpers;

public interface IPkceValidationHelper
{
    bool Validate(string verifier, string challenge, string method);
}

public class PkceValidationHelper : IPkceValidationHelper
{
    public bool Validate(string verifier, string challenge, string method)
    {
        return method switch
        {
            "plain" => ValidatePlain(verifier, challenge),
            "S256" => ValidateSHA256(verifier, challenge),
            _ => throw new OAuthException("invalid_grant")
        };
    }

    private bool ValidatePlain(string verifier, string challenge)
    {
        if (!string.Equals(verifier, challenge, StringComparison.Ordinal))
            throw new OAuthException("invalid_grant");

        return true;
    }

    private bool ValidateSHA256(string verifier, string challenge)
    {
        using var sha256 = SHA256.Create();

        var bytes = Encoding.UTF8.GetBytes(verifier);
        var hash = sha256.ComputeHash(bytes);

        var computedChallenge = Convert.ToBase64String(hash)
                                       .Replace("+", "-")
                                       .Replace("/", "_")
                                       .Replace("=", "");

        if (!string.Equals(computedChallenge, challenge, StringComparison.Ordinal))
            throw new OAuthException("invalid_grant");

        return true;
    }
}
