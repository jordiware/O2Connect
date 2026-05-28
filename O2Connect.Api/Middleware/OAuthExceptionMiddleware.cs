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
            _logger.LogDebug("OAuth error: {Error} - {Description}", ex.Error, ex.Description);
            _logger.LogWarning("OAuth error: {Error}", ex.Error);

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
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = ex.StatusCode;
        context.Response.ContentType = "application/json";
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers.Pragma = "no-cache";

        if (!string.IsNullOrWhiteSpace(ex.WwwAuthenticate))
        {
            context.Response.Headers["WWW-Authenticate"] = ex.WwwAuthenticate;
        }

        var payload = new OAuthErrorResponse
        {
            Error = ex.Error,
            ErrorDescription = ex.Error == "invalid_client" ? null : ex.Description,
            ErrorUri = ex.ErrorUri
        };

        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, jsonOptions),
                                          context.RequestAborted);
    }

    private static async Task HandleGenericExceptionAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers.Pragma = "no-cache";

        var payload = new
        {
            error = "internal_server_error"
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload),
                                          context.RequestAborted);
    }
}
