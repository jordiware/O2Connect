using Microsoft.Extensions.Options;
using O2Connect.Api.Config.OptionsModels;

namespace O2Connect.Api.Config;

public interface IDiscoveryEndpointsConfig
{
    string Documentation { get; }
    string PrivacyPolicy { get; }
    string TermsOfService { get; }
}

public sealed class DiscoveryEndpointsConfig : IDiscoveryEndpointsConfig
{
    private readonly DiscoveryEndpoints _endpoints;

    public DiscoveryEndpointsConfig(IOptions<DiscoveryEndpoints> endpoints)
    {
        _endpoints = endpoints.Value;
    }

    public string Documentation => _endpoints.Documentation;
    public string PrivacyPolicy => _endpoints.PrivacyPolicy;
    public string TermsOfService => _endpoints.TermsOfService;
}
