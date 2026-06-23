using Microsoft.Extensions.Options;
using O2Connect.Api.Config.OptionsModels;
using O2Connect.Api.Config.Validators;

namespace O2Connect.Api.Config;

public sealed class ConfigValidationStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            using var scope = app.ApplicationServices.CreateScope();
            var provider = scope.ServiceProvider;

            ValidateSection<ApiOptions>(provider);
            ValidateSection<DatabaseOptions>(provider);
            ValidateSection<DiscoveryEndpoints>(provider);
            ValidateSection<FeaturesOptions>(provider);
            ValidateSection<JwtOptions>(provider);
            ValidateSection<OAuthOptions>(provider);
            ValidateSection<OidcOptions>(provider);
            ValidateSection<SecurityOptions>(provider);

            ValidateCrossConfig(provider);

            next(app);
        };
    }

    private void ValidateSection<T>(IServiceProvider provider)
        where T : class
    {
        var options = provider.GetRequiredService<IOptions<T>>().Value;
        var validators = provider.GetServices<IConfigValidator<T>>();

        foreach (var validator in validators)
        {
            validator.Validate(options);
        }
    }

    private void ValidateCrossConfig(IServiceProvider provider)
    {
        var api = provider.GetRequiredService<IOptions<ApiOptions>>().Value;
        var jwt = provider.GetRequiredService<IOptions<JwtOptions>>().Value;
        var oidc = provider.GetRequiredService<IOptions<OidcOptions>>().Value;

        new CoreConsistencyValidator().Validate(api, jwt, oidc);
    }

}
