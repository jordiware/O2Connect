using O2Connect.Api.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace O2Connect.Api.Services.Helpers;

public interface IPkceHelper
{
    bool Validate(string verifier, string challenge, string method);
}

public class PkceHelper : IPkceHelper
{
    public bool Validate(string verifier, string challenge, string method)
    {
        using var crypto = CryptoConfig.CreateFromName(method) as IDisposable;
        return crypto switch
        {
            SHA256 sha256 => ValidateSHA256(sha256, verifier, challenge),
            _ => throw new OAuthException("invalid_grant")
        };
    }

    private bool ValidateSHA256(SHA256 cryptoHelper, string verifier, string challenge)
    {
        var bytes = Encoding.UTF8.GetBytes(verifier);
        var hash = cryptoHelper.ComputeHash(bytes);
        var computedChallenge = Convert.ToBase64String(hash)
                                       .Replace("+", "-")
                                       .Replace("/", "_")
                                       .Replace("=", "");

        if (!string.Equals(computedChallenge, challenge, StringComparison.Ordinal))
            throw new OAuthException("invalid_grant");

        return true;
    }
}
