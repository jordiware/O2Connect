using O2Connect.Api.Models.Options;

namespace O2Connect.Api.DataValidators.ConfigValidators;

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
