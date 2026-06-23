using Microsoft.Extensions.Options;
using O2Connect.Api.Config.Options;

namespace O2Connect.Api.Config;

public interface IFeaturesConfig
{
    bool EnableDeviceFlow { get; }
    bool EnableDynamicClientRegistration { get; }
    bool EnableRevocationEndpoint { get; }
}

public sealed class FeaturesConfig : IFeaturesConfig
{
    private readonly FeaturesOptions _options;

    public FeaturesConfig(IOptions<FeaturesOptions> options)
    {
        _options = options.Value;
    }

    public bool EnableDynamicClientRegistration => _options.EnableDynamicClientRegistration;
    public bool EnableDeviceFlow => _options.EnableDeviceFlow;
    public bool EnableRevocationEndpoint => _options.EnableRevocationEndpoint;
}
