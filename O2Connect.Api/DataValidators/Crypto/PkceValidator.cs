using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.DataValidators.Crypto;

public abstract class PkceValidator : IPkceValidator
{
    public abstract PkceMethod Method { get; }

    protected abstract bool Validate(string verifier, string challenge);

    public virtual void Validate(Client client, AuthorizationCode code, string? codeVerifier)
    {
        if (string.IsNullOrEmpty(code.CodeChallenge) && client.RequiresPkce)
            throw OAuthException.FromInvalidGrant("PKCE required");

        if (string.IsNullOrEmpty(codeVerifier))
            throw OAuthException.FromInvalidGrant("Missing code_verifier");

        if (!Validate(codeVerifier, code.CodeChallenge!))
            throw OAuthException.FromInvalidGrant("Invalid code_verifier");
    }
}
