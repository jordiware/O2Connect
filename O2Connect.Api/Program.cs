using O2Connect.Api.Middleware;
using O2Connect.Api.Repositories;
using O2Connect.Api.Services;
using O2Connect.Api.Services.PkceValidators;
using O2Connect.Api.Services.TokenGrantHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IClientRepository, InMemoryClientRepository>();
builder.Services.AddSingleton<IAuthorizationCodeRepository, InMemoryAuthorizationCodeRepository>();

builder.Services.AddScoped<IPkceValidator, PlainPkceValidator>();
builder.Services.AddScoped<IPkceValidator, S256PkceValidator>();

builder.Services.AddScoped<ITokenGrantHandler, AuthorizationCodeGrantHandler>();
builder.Services.AddScoped<ITokenGrantHandler, ClientCredentialsGrantHandler>();
builder.Services.AddScoped<ITokenGrantHandler, RefreshTokenGrantHandler>();

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
