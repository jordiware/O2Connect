namespace O2Connect.Api.Services.Validators;

public interface IPkceValidator
{
    string Method { get; }
    bool Validate(string verifier, string challenge);
}
