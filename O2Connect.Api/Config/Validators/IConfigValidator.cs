namespace O2Connect.Api.Config.Validators;

public interface IConfigValidator<in TOptions>
{
    void Validate(TOptions options);
}
