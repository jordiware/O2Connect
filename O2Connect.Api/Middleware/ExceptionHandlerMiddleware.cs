using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using O2Connect.Api.Config.OptionsModels;
using O2Connect.Api.Exceptions;
using O2Connect.Dto.OidcOAuth;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace O2Connect.Api.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiOptions _apiOptions;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        IOptions<ApiOptions> apiOptions,
        ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _apiOptions = apiOptions.Value;
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
        catch (ApiException ex)
        {
            var problem = new ProblemDetails
            {
                Type = $"{_apiOptions.Domain}{ex.Type}",
                Title = ex.Title,
                Status = ex.StatusCode,
                Detail = ex.Message,
                Instance = context.Request.Path
            };

            problem.Extensions["code"] = ex.ErrorCode;
            problem.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsJsonAsync(problem);
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
            ErrorDescription = string.Equals(ex.Error, "invalid_client", StringComparison.Ordinal) 
                               ? null 
                               : ex.Description,
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
