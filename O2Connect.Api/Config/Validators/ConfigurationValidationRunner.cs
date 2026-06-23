using Microsoft.Extensions.Options;
using O2Connect.Api.Config.OptionsModels;
using O2Connect.Api.DataValidators.ConfigValidators;

namespace O2Connect.Api.Config.Validators;

public sealed class ConfigurationValidationRunner
{
    private readonly IServiceProvider _provider;

    public ConfigurationValidationRunner(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void Validate()
    {
        ValidateSection<ApiOptions>();
        ValidateSection<DatabaseOptions>();
        ValidateSection<DiscoveryEndpoints>();
        ValidateSection<FeaturesOptions>();
        ValidateSection<JwtOptions>();
        ValidateSection<OAuthOptions>();
        ValidateSection<OidcOptions>();
        ValidateSection<SecurityOptions>();

        ValidateCrossConfig();
    }

    private void ValidateSection<T>() 
        where T : class
    {
        var options = _provider.GetRequiredService<IOptions<T>>().Value;
        var validators = _provider.GetServices<IConfigValidator<T>>();

        foreach (var validator in validators)
        {
            validator.Validate(options);
        }
    }

    private void ValidateCrossConfig()
    {
        var api = _provider.GetRequiredService<IOptions<ApiOptions>>().Value;
        var jwt = _provider.GetRequiredService<IOptions<JwtOptions>>().Value;
        var oidc = _provider.GetRequiredService<IOptions<OidcOptions>>().Value;

        new CoreConsistencyValidator().Validate(api, jwt, oidc);
    }
}
