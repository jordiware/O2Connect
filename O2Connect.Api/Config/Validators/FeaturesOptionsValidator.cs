using O2Connect.Api.Config.Options;

namespace O2Connect.Api.Config.Validators;

public sealed class FeaturesOptionsValidator : IConfigValidator<FeaturesOptions>
{
    public void Validate(FeaturesOptions options)
    {
        // No strict validation required yet.
        // This is intentional: feature flags are meant to be flexible.

        // Future example:
        // if (!options.EnableDeviceFlow && something depends on it)
        //     throw new InvalidOperationException(...);
    }
}
