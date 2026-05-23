using O2Connect.Api.Models;

namespace O2Connect.Api.Crypto.Validators;

public interface IPkceValidator
{
    PkceMethod Method { get; }
    bool Validate(string verifier, string challenge);
}
