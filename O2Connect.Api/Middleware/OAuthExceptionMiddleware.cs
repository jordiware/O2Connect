using O2Connect.Api.Exceptions;
using O2Connect.Dto.Responses;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace O2Connect.Api.Middleware;

public class OAuthExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<OAuthExceptionMiddleware> _logger;

    public OAuthExceptionMiddleware(
        RequestDelegate next,
        ILogger<OAuthExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OAuthException ex)
        {
            _logger.LogWarning("OAuth error: {Error} - {Description}", ex.Error, ex.Description);

            await HandleOAuthExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            await HandleGenericExceptionAsync(context);
        }
    }

    private static async Task HandleOAuthExceptionAsync(HttpContext context, OAuthException ex)
    {
        context.Response.StatusCode = ex.StatusCode;
        context.Response.ContentType = "application/json";

        if (!string.IsNullOrWhiteSpace(ex.WwwAuthenticate))
        {
            context.Response.Headers["WWW-Authenticate"] = ex.WwwAuthenticate;
        }

        var payload = new OAuthErrorResponse
        {
            Error = ex.Error,
            ErrorDescription = ex.Description,
            ErrorUri = ex.ErrorUri
        };

        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        if (context.Response.HasStarted)
            return;

        await context.Response.WriteAsJsonAsync(payload, jsonOptions, context.RequestAborted);
    }

    private static async Task HandleGenericExceptionAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            error = "internal_server_error"
        };

        if (context.Response.HasStarted)
            return;

        await context.Response.WriteAsJsonAsync(payload, context.RequestAborted);
    }
}
