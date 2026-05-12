using O2Connect.Api.Exceptions;

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
        context.Response.StatusCode = MapStatusCode(ex.Error);
        context.Response.ContentType = "application/json";

        if (ex.Error == "invalid_client")
        {
            context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"token\"";
        }

        var payload = new
        {
            error = ex.Error,
            error_description = ex.Description
        };

        await context.Response.WriteAsJsonAsync(payload);
    }

    private static async Task HandleGenericExceptionAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            error = "server_error"
        };

        await context.Response.WriteAsJsonAsync(payload);
    }

    private static int MapStatusCode(string error) => error switch
    {
        "invalid_client" => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status400BadRequest
    };
}
