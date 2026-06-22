using O2Connect.Api.Config.OptionsModels;

namespace O2Connect.Api.Config.Validators;

public sealed class FeatureOptionsValidator : IConfigValidator<FeatureOptions>
{
    public void Validate(FeatureOptions options)
    {
        // No strict validation required yet.
        // This is intentional: feature flags are meant to be flexible.

        // Future example:
        // if (!options.EnableDeviceFlow && something depends on it)
        //     throw new InvalidOperationException(...);
    }
}
