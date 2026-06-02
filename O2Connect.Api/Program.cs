using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Controllers.RequestModelValidators;
using O2Connect.Api.Crypto;
using O2Connect.Api.DataFactories;
using O2Connect.Api.DataHandlers.ClientAuthentication;
using O2Connect.Api.DataHandlers.TokenGrantHandlers;
using O2Connect.Api.DataValidators;
using O2Connect.Api.Middleware;
using O2Connect.Api.Models.Options;
using O2Connect.Api.Repositories;
using O2Connect.Api.Repositories.Cache;
using O2Connect.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<OAuthOptions>(builder.Configuration.GetSection("OAuth"));
builder.Services.Configure<DiscoveryEndpoints>(builder.Configuration.GetSection(DiscoveryEndpoints.SectionName));

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000;
});

builder.Services.AddSingleton<IReplayCache, MemoryReplayCache>(); 
builder.Services.AddSingleton<IClientRepository, InMemoryClientRepository>();
builder.Services.AddSingleton<IAuthorizationCodeRepository, InMemoryAuthorizationCodeRepository>();
builder.Services.AddSingleton<IAuthorizationSessionRepository, InMemoryAuthorizationSessionRepository>();
builder.Services.AddSingleton<IRefreshTokenRepository, InMemoryRefreshTokenRepository>();
builder.Services.AddSingleton<IUserConsentRepository, InMemoryUserConsentRepository>();

builder.Services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
builder.Services.AddSingleton<IJwksProvider, JwksProvider>();
builder.Services.AddSingleton<ISigningKeyProvider, RsaSigningKeyProvider>();
builder.Services.AddSingleton<ITokenFactory, JwtTokenFactory>();

builder.Services.AddTransient<ITokenGrantHandler, AuthorizationCodeGrantHandler>();
builder.Services.AddTransient<ITokenGrantHandler, ClientCredentialsGrantHandler>();
builder.Services.AddTransient<ITokenGrantHandler, RefreshTokenGrantHandler>();
builder.Services.AddSingleton<ITokenGrantHandlerResolver, TokenGrantHandlerResolver>();

builder.Services.AddTransient<ITokenRequestValidator, AuthorizationCodeTokenRequestValidator>();
builder.Services.AddTransient<ITokenRequestValidator, ClientCredentialsTokenRequestValidator>();
builder.Services.AddTransient<ITokenRequestValidator, RefreshTokenTokenRequestValidator>();
builder.Services.AddSingleton<ITokenRequestValidatorResolver, TokenRequestValidatorResolver>();

builder.Services.AddScoped<IClientAuthenticationHandler, ClientSecretBasicHandler>();
builder.Services.AddScoped<IClientAuthenticationHandler, ClientSecretPostHandler>();
builder.Services.AddScoped<IClientAuthenticationHandler, PrivateKeyJwtHandler>();

builder.Services.AddTransient<ISecretHasher, Pbkdf2SecretHasher>();

builder.Services.AddScoped<IClientAuthenticationService, ClientAuthenticationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<IConsentService, ConsentService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<OAuthExceptionMiddleware>();

app.MapControllers();

app.Run();
