namespace O2Connect.Api.Services.PkceValidators;

public interface IPkceValidator
{
    string Method { get; }
    bool Validate(string verifier, string challenge);
}
