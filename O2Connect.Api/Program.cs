using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Crypto;
using O2Connect.Api.DataFactories;
using O2Connect.Api.DataHandlers.ClientAuthentication;
using O2Connect.Api.DataHandlers.TokenContextHandlers;
using O2Connect.Api.DataValidators;
using O2Connect.Api.DataValidators.TokenRequestValidators;
using O2Connect.Api.Middleware;
using O2Connect.Api.Models.Options;
using O2Connect.Api.Repositories;
using O2Connect.Api.Repositories.Cache;
using O2Connect.Api.Repositories.InMemoryRepositories;
using O2Connect.Api.Security;
using O2Connect.Api.Services;
using O2Connect.Api.Services.Management;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.SectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<OAuthOptions>(builder.Configuration.GetSection(OAuthOptions.SectionName));
builder.Services.Configure<DiscoveryEndpoints>(builder.Configuration.GetSection(DiscoveryEndpoints.SectionName));

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000;
});

builder.Services.AddAuthorizationBuilder()
                .SetDefaultPolicy(new AuthorizationPolicyBuilder("Bearer").RequireAuthenticatedUser().Build());

builder.Services.AddSingleton<IAuthorizationHandler, ScopeAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, ScopePolicyProvider>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(RequireClientTokenAttribute.PolicyName, policy =>
    {
        policy.RequireAssertion(ctx => ctx.User.IsClientToken());
    });
    options.AddPolicy(RequireUserTokenAttribute.PolicyName, policy =>
    {
        policy.RequireAssertion(ctx => ctx.User.IsUserToken());
    });
});

builder.Services.AddSingleton<IReplayCache, MemoryReplayCache>();
builder.Services.AddSingleton<ITokenReplayCache, MemoryTokenReplayCache>();

builder.Services.AddSingleton<IClientRepository, MemoryClientRepository>();
builder.Services.AddSingleton<IAuthorizationCodeRepository, MemoryAuthorizationCodeRepository>();
builder.Services.AddSingleton<IAuthorizationSessionRepository, MemoryAuthorizationSessionRepository>();
builder.Services.AddSingleton<IRefreshTokenRepository, MemoryRefreshTokenRepository>();
builder.Services.AddSingleton<IUserConsentRepository, MemoryUserConsentRepository>();
builder.Services.AddSingleton<IUserRepository, MemoryUserRepository>();
builder.Services.AddSingleton<IParEntryRepository, MemoryParEntryRepository>();
builder.Services.AddSingleton<IDeviceAuthorizationRepository, MemoryDeviceAuthorizationRepository>();

builder.Services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
builder.Services.AddSingleton<IJwksProvider, JwksProvider>();
builder.Services.AddSingleton<IJwtValidator, JwtValidator>();
builder.Services.AddSingleton<ISigningKeyProvider, RsaSigningKeyProvider>();
builder.Services.AddSingleton<ITokenFactory, JwtTokenFactory>();
builder.Services.AddSingleton<ISecretHasher, Pbkdf2SecretHasher>();
builder.Services.AddSingleton<IPushedAuthorizationValidator, PushedAuthorizationValidator>();

builder.Services.AddSingleton<IClientsQueryValidator, ClientsQueryValidator>();

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

builder.Services.AddTransient<ITokenContextHandler, AuthorizationCodeContextHandler>();
builder.Services.AddTransient<ITokenContextHandler, ClientCredentialsContextHandler>();
builder.Services.AddTransient<ITokenContextHandler, RefreshTokenContextHandler>();
builder.Services.AddTransient<ITokenContextHandler, DeviceCodeContextHandler>();
builder.Services.AddSingleton<ITokenContextHandlerResolver, TokenContextHandlerResolver>();

builder.Services.AddTransient<ITokenRequestValidator, AuthorizationCodeTokenRequestValidator>();
builder.Services.AddTransient<ITokenRequestValidator, ClientCredentialsTokenRequestValidator>();
builder.Services.AddTransient<ITokenRequestValidator, RefreshTokenTokenRequestValidator>();
builder.Services.AddTransient<ITokenRequestValidator, DeviceCodeTokenRequestValidator>();
builder.Services.AddSingleton<ITokenRequestValidatorResolver, TokenRequestValidatorResolver>();

builder.Services.AddScoped<IClientAuthenticationHandler, ClientSecretBasicHandler>();
builder.Services.AddScoped<IClientAuthenticationHandler, ClientSecretPostHandler>();
builder.Services.AddScoped<IClientAuthenticationHandler, PrivateKeyJwtHandler>();

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

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.MapControllers();

app.Run();
