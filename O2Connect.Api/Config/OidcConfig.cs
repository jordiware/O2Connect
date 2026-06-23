using Microsoft.Extensions.Options;
using O2Connect.Api.Config.OptionsModels;

namespace O2Connect.Api.Config;

public interface IOidcConfig
{
    string AuthorizationEndpoint { get; }
    string EndSessionEndpoint { get; }
    string Issuer { get; }
    string JwksEndpoint { get; }
    IReadOnlyCollection<string> ResponseTypesSupported { get; }
    IReadOnlyCollection<string> ScopesSupported { get; }
    string TokenEndpoint { get; }
    string UserInfoEndpoint { get; }
}

public sealed class OidcConfig : IOidcConfig
{
    private readonly OidcOptions _options;

    public OidcConfig(IOptions<OidcOptions> options)
    {
        _options = options.Value;
    }

    public string Issuer => _options.Issuer;
    public string AuthorizationEndpoint => _options.AuthorizationEndpoint;
    public string TokenEndpoint => _options.TokenEndpoint;
    public string UserInfoEndpoint => _options.UserInfoEndpoint;
    public string JwksEndpoint => _options.JwksEndpoint;
    public string EndSessionEndpoint => _options.EndSessionEndpoint;
    public IReadOnlyCollection<string> ScopesSupported => _options.ScopesSupported;
    public IReadOnlyCollection<string> ResponseTypesSupported => _options.ResponseTypesSupported;
}
