using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Crypto;
using O2Connect.Api.DataFactories;
using O2Connect.Api.DataHandlers.ClientAuthentication;
using O2Connect.Api.DataHandlers.TokenContextHandlers;
using O2Connect.Api.DataValidators;
using O2Connect.Api.DataValidators.ConfigValidators;
using O2Connect.Api.DataValidators.TokenRequestValidators;
using O2Connect.Api.Middleware;
using O2Connect.Api.Models.Options;
using O2Connect.Api.Persistence;
using O2Connect.Api.Repositories;
using O2Connect.Api.Repositories.Cache;
using O2Connect.Api.Repositories.DatabaseRepositories;
using O2Connect.Api.Security;
using O2Connect.Api.Services;
using O2Connect.Api.Services.Management;
using O2Connect.Api.Services.OidcOAuth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000;
});

builder.Services.AddAuthorizationBuilder()
                .AddPolicy(RequireClientTokenAttribute.PolicyName, policy =>
                {
                    policy.RequireAssertion(ctx => ctx.User.IsClientToken());
                })
                .AddPolicy(RequireUserTokenAttribute.PolicyName, policy =>
                {
                    policy.RequireAssertion(ctx => ctx.User.IsUserToken());
                })
                .SetDefaultPolicy(new AuthorizationPolicyBuilder("Bearer").RequireAuthenticatedUser()
                                                                          .Build());

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

#region APP_OPTIONS
builder.Services.AddSingleton<IConfigValidator<ApiOptions>, ApiOptionsValidator>();
builder.Services.AddSingleton<IConfigValidator<DatabaseOptions>, DatabaseOptionsValidator>();
builder.Services.AddSingleton<IConfigValidator<DiscoveryEndpoints>, DiscoveryEndpointsValidator>();
builder.Services.AddSingleton<IConfigValidator<FeatureOptions>, FeatureOptionsValidator>();
builder.Services.AddSingleton<IConfigValidator<JwtOptions>, JwtOptionsValidator>();
builder.Services.AddSingleton<IConfigValidator<OAuthOptions>, OAuthOptionsValidator>();
builder.Services.AddSingleton<IConfigValidator<OidcOptions>, OidcOptionsValidator>();
builder.Services.AddSingleton<IConfigValidator<SecurityOptions>, SecurityOptionsValidator>();

builder.Services.AddSingleton<ConfigurationValidationRunner>();

builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.SectionName));
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
builder.Services.Configure<DiscoveryEndpoints>(builder.Configuration.GetSection(DiscoveryEndpoints.SectionName));
builder.Services.Configure<FeatureOptions>(builder.Configuration.GetSection(FeatureOptions.SectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<OAuthOptions>(builder.Configuration.GetSection(OAuthOptions.SectionName));
builder.Services.Configure<OidcOptions>(builder.Configuration.GetSection(OidcOptions.SectionName));
builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection(SecurityOptions.SectionName));
#endregion

#region DATA_PERSISTENCE
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDb")));

builder.Services.AddScoped<IClientRepository, DbClientRepository>();
builder.Services.AddScoped<IAuthorizationCodeRepository, DbAuthorizationCodeRepository>();
builder.Services.AddScoped<IAuthorizationSessionRepository, DbAuthorizationSessionRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, DbRefreshTokenRepository>();
builder.Services.AddScoped<IUserConsentRepository, DbUserConsentRepository>();
builder.Services.AddScoped<IUserRepository, DbUserRepository>();
builder.Services.AddScoped<IParEntryRepository, DbParEntryRepository>();
builder.Services.AddScoped<IDeviceAuthorizationRepository, DbDeviceAuthorizationRepository>();
#endregion

builder.Services.AddSingleton<IAuthorizationHandler, ScopeAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, ScopePolicyProvider>();

builder.Services.AddSingleton<IReplayCache, MemoryReplayCache>();
builder.Services.AddSingleton<ITokenReplayCache, MemoryTokenReplayCache>();

builder.Services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
builder.Services.AddSingleton<IJwksProvider, JwksProvider>();
builder.Services.AddSingleton<IJwtValidator, JwtValidator>();
builder.Services.AddSingleton<ISigningKeyProvider, RsaSigningKeyProvider>();
builder.Services.AddSingleton<ITokenFactory, JwtTokenFactory>();
builder.Services.AddSingleton<ISecretHasher, Pbkdf2SecretHasher>();
builder.Services.AddSingleton<IPushedAuthorizationValidator, PushedAuthorizationValidator>();

builder.Services.AddSingleton<IClientSecretValidator, ClientSecretValidator>();
builder.Services.AddSingleton<IPaginationQueryValidator, PaginationQueryValidator>();
builder.Services.AddSingleton<IClientsQueryValidator, ClientsQueryValidator>();
builder.Services.AddSingleton<IUsersQueryValidator, UsersQueryValidator>();

builder.Services.AddSingleton(sp =>
{
    var keys = sp.GetRequiredService<ISigningKeyProvider>();

    return new TokenValidationParameters
    {
        ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
        RequireSignedTokens = true,

        NameClaimType = "sub",
        RoleClaimType = "role",

        ValidateIssuer = true,
        ValidIssuers = ["your-issuer"],

        ValidateAudience = true,
        ValidAudiences = ["your-audience"],

        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.FromSeconds(30),

        ValidateTokenReplay = true,
        TokenReplayCache = sp.GetRequiredService<ITokenReplayCache>(),

        ValidateIssuerSigningKey = true,
        IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
        {
            if (string.IsNullOrWhiteSpace(kid))
                return [];

            var signingKeys = keys.GetValidSigningKeys();

            var matches = signingKeys.Where(k => k.Key.KeyId == kid)
                                     .Select(k => k.Key)
                                     .ToList();

            if (matches.Count == 1)
                return matches;

            return [];
        }
    };
});

builder.Services.AddScoped<ITokenContextHandler, AuthorizationCodeContextHandler>();
builder.Services.AddScoped<ITokenContextHandler, ClientCredentialsContextHandler>();
builder.Services.AddScoped<ITokenContextHandler, RefreshTokenContextHandler>();
builder.Services.AddScoped<ITokenContextHandler, DeviceCodeContextHandler>();
builder.Services.AddScoped<ITokenContextHandlerResolver, TokenContextHandlerResolver>();

builder.Services.AddScoped<ITokenRequestValidator, AuthorizationCodeTokenRequestValidator>();
builder.Services.AddScoped<ITokenRequestValidator, ClientCredentialsTokenRequestValidator>();
builder.Services.AddScoped<ITokenRequestValidator, RefreshTokenTokenRequestValidator>();
builder.Services.AddScoped<ITokenRequestValidator, DeviceCodeTokenRequestValidator>();
builder.Services.AddScoped<ITokenRequestValidatorResolver, TokenRequestValidatorResolver>();

builder.Services.AddScoped<IClientAuthenticationHandler, ClientSecretBasicHandler>();
builder.Services.AddScoped<IClientAuthenticationHandler, ClientSecretPostHandler>();
builder.Services.AddScoped<IClientAuthenticationHandler, PrivateKeyJwtHandler>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IClientAuthenticationService, ClientAuthenticationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IConnectAuthorizationService, ConnectAuthorizationService>();
builder.Services.AddScoped<IConsentService, ConsentService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserInfoService, UserInfoService>();
builder.Services.AddScoped<ITokenIntrospectionService, TokenIntrospectionService>();
builder.Services.AddScoped<IRevocationService, RevocationService>();
builder.Services.AddScoped<IDiscoveryMetadataService, DiscoveryMetadataService>();
builder.Services.AddScoped<IPushedAuthorizationService, PushedAuthorizationService>();
builder.Services.AddScoped<IParAuthorizationService, ParAuthorizationService>();
builder.Services.AddScoped<IClientRegistrationService, ClientRegistrationService>();
builder.Services.AddScoped<IDeviceConnectService, DeviceConnectService>();

builder.Services.AddScoped<IManagementClientsService, ManagementClientsService>();
builder.Services.AddScoped<IManagementUsersService, ManagementUsersService>();
builder.Services.AddScoped<IManagementProfileService, ManagementProfileService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var validator = scope.ServiceProvider
        .GetRequiredService<ConfigurationValidationRunner>();

    validator.Validate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.MapControllers();

app.Run();
