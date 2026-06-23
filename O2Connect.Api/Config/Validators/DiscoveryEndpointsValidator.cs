using O2Connect.Api.Config.Options;

namespace O2Connect.Api.Config.Validators;

public sealed class DiscoveryEndpointsValidator : IConfigValidator<DiscoveryEndpoints>
{
    public void Validate(DiscoveryEndpoints options)
    {
        ValidateUrl(options.Documentation, "Documentation");
        ValidateUrl(options.PrivacyPolicy, "PrivacyPolicy");
        ValidateUrl(options.TermsOfService, "TermsOfService");
    }

    private static void ValidateUrl(string value, string name)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out _))
            throw new InvalidOperationException($"DiscoveryEndpoints:{name} must be a valid absolute URI.");
    }
}
