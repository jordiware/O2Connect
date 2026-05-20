using O2Connect.Api.Crypto;
using O2Connect.Api.Crypto.Validators;
using O2Connect.Api.DataHandlers.TokenGrantHandlers;
using O2Connect.Api.DataValidators;
using O2Connect.Api.Middleware;
using O2Connect.Api.Repositories;
using O2Connect.Api.Services;
using O2Connect.Api.Services.Authenticators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IClientRepository, InMemoryClientRepository>();
builder.Services.AddSingleton<IAuthorizationCodeRepository, InMemoryAuthorizationCodeRepository>();

builder.Services.AddTransient<IPkceValidator, PlainPkceValidator>();
builder.Services.AddTransient<IPkceValidator, S256PkceValidator>();
builder.Services.AddScoped<IPkceValidatorResolver, PkceValidatorResolver>();

builder.Services.AddTransient<ISecretHasher, Pbkdf2SecretHasher>();

builder.Services.AddTransient<ITokenGrantHandler, AuthorizationCodeGrantHandler>();
builder.Services.AddTransient<ITokenGrantHandler, ClientCredentialsGrantHandler>();
builder.Services.AddTransient<ITokenGrantHandler, RefreshTokenGrantHandler>();
builder.Services.AddScoped<ITokenGrantHandlerResolver, TokenGrantHandlerResolver>();

builder.Services.AddScoped<IScopeValidator, ScopeValidator>();
builder.Services.AddScoped<ITokenRequestValidator, TokenRequestValidator>();

builder.Services.AddScoped<IClientAuthenticator, ClientAuthenticator>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

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
