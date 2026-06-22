namespace O2Connect.Api.DataValidators.ConfigValidators;

public interface IConfigValidator<in TOptions>
{
    void Validate(TOptions options);
}
