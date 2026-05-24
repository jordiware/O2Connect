using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Crypto.Validators;

public interface IPkceValidator
{
    PkceMethod Method { get; }
    void Validate(Client client, AuthorizationCode code, string? codeVerifier);
}
