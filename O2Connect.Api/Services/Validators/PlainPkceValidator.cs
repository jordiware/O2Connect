using O2Connect.Api.Exceptions;

namespace O2Connect.Api.Services.Validators;

public class PlainPkceValidator : IPkceValidator
{
    public string Method => "plain";

    public bool Validate(string verifier, string challenge)
    {
        if (!string.Equals(verifier, challenge, StringComparison.Ordinal))
            throw new OAuthException("invalid_grant");

        return true;
    }
}
