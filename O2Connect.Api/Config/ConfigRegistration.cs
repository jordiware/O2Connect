using O2Connect.Api.Config.OptionsModels;
using O2Connect.Api.Config.Validators;

namespace O2Connect.Api.Config;

public static class ConfigRegistration
{
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services,
                                                         IConfiguration configuration)
    {
        services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.SectionName));
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<DiscoveryEndpoints>(configuration.GetSection(DiscoveryEndpoints.SectionName));
        services.Configure<FeaturesOptions>(configuration.GetSection(FeaturesOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<OAuthOptions>(configuration.GetSection(OAuthOptions.SectionName));
        services.Configure<OidcOptions>(configuration.GetSection(OidcOptions.SectionName));
        services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));

        services.AddSingleton<IApiConfig, ApiConfig>();
        services.AddSingleton<IDatabaseConfig, DatabaseConfig>();
        services.AddSingleton<IDiscoveryEndpointsConfig, DiscoveryEndpointsConfig>();
        services.AddSingleton<IFeaturesConfig, FeaturesConfig>();
        services.AddSingleton<IJwtConfig, JwtConfig>();
        services.AddSingleton<IOAuthConfig, OAuthConfig>();
        services.AddSingleton<IOidcConfig, OidcConfig>();
        services.AddSingleton<ISecurityConfig, SecurityConfig>();

        services.AddSingleton<IConfigValidator<ApiOptions>, ApiOptionsValidator>();
        services.AddSingleton<IConfigValidator<DatabaseOptions>, DatabaseOptionsValidator>();
        services.AddSingleton<IConfigValidator<DiscoveryEndpoints>, DiscoveryEndpointsValidator>();
        services.AddSingleton<IConfigValidator<FeaturesOptions>, FeaturesOptionsValidator>();
        services.AddSingleton<IConfigValidator<JwtOptions>, JwtOptionsValidator>();
        services.AddSingleton<IConfigValidator<OAuthOptions>, OAuthOptionsValidator>();
        services.AddSingleton<IConfigValidator<OidcOptions>, OidcOptionsValidator>();
        services.AddSingleton<IConfigValidator<SecurityOptions>, SecurityOptionsValidator>();

        services.AddSingleton<IStartupFilter, ConfigValidationStartupFilter>();

        return services;
    }
}
