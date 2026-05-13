using O2Connect.Api.Middleware;
using O2Connect.Api.Repositories;
using O2Connect.Api.Services;
using O2Connect.Api.Services.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IPkceValidationHelper, PkceValidationHelper>();

builder.Services.AddSingleton<IClientRepository, InMemoryClientRepository>();
builder.Services.AddSingleton<IAuthorizationCodeRepository, InMemoryAuthorizationCodeRepository>();

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
