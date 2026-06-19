namespace O2Connect.Api.Exceptions;

public sealed class OAuthException : Exception
{
    public static OAuthException FromAccessDenied(string? description = null, string? errorUri = null) =>
        new OAuthException("access_denied", StatusCodes.Status403Forbidden, description, errorUri);

    public static OAuthException FromAuthorizationPending(string? description = null, string? errorUri = null) =>
        new OAuthException("authorization_pending", StatusCodes.Status400BadRequest, description, errorUri);

    public static OAuthException FromInvalidGrant(string? description = null, string? errorUri = null) =>
        new OAuthException("invalid_grant", StatusCodes.Status400BadRequest, description, errorUri);

    public static OAuthException FromInvalidRequest(string? description = null, string? errorUri = null) =>
        new OAuthException("invalid_request", StatusCodes.Status400BadRequest, description, errorUri);

    public static OAuthException FromInvalidScope(string? description = null, string? errorUri = null) =>
        new OAuthException("invalid_scope", StatusCodes.Status400BadRequest, description, errorUri);

    public static OAuthException FromInvalidRedirectUri(string? description = null, string? errorUri = null) =>
        new OAuthException("invalid_redirect_uri", StatusCodes.Status400BadRequest, description, errorUri);

    public static OAuthException FromServerError(string? description = null, string? errorUri = null) =>
        new OAuthException("server_error", StatusCodes.Status500InternalServerError, description, errorUri);

    public static OAuthException FromSlowDown(string? description = null, string? errorUri = null) =>
        new OAuthException("slow_down", StatusCodes.Status400BadRequest, description, errorUri);

    public static OAuthException FromTemporarilyUnavailable(string? description = null, string? errorUri = null) =>
        new OAuthException("temporarily_unavailable", StatusCodes.Status503ServiceUnavailable, description, errorUri);
    
    public static OAuthException FromUnauthorizedClient(string? description = null, string? errorUri = null) =>
        new OAuthException("unauthorized_client", StatusCodes.Status400BadRequest, description, errorUri);
    
    public static OAuthException FromUnsupportedGrantType(string? description = null, string? errorUri = null) =>
        new OAuthException("unsupported_grant_type", StatusCodes.Status400BadRequest, description, errorUri);
    
    public static OAuthException FromUnsupportedResponseType(string? description = null, string? errorUri = null) =>
        new OAuthException("unsupported_response_type", StatusCodes.Status400BadRequest, description, errorUri);

    public static OAuthException FromInvalidClient(
        string? description = null,
        string? errorUri = null,
        string? authenticationScheme = null,
        string? realm = null)
    {
        authenticationScheme ??= "Basic";
        realm ??= "token";

        var parts = new List<string>
        {
            $"realm=\"{Escape(realm)}\"",
            $"error=\"invalid_client\""
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            parts.Add($"error_description=\"{Escape(description)}\"");
        }

        if (!string.IsNullOrWhiteSpace(errorUri))
        {
            parts.Add($"error_uri=\"{Escape(errorUri)}\"");
        }

        var wwwAuthenticate = $"{authenticationScheme} {string.Join(", ", parts)}";

        return new OAuthException(
            "invalid_client",
            StatusCodes.Status401Unauthorized,
            description,
            errorUri,
            wwwAuthenticate);
    }

    public static OAuthException FromInvalidToken(
        string? description = null,
        string? errorUri = null,
        string? realm = null)
    {
        realm ??= "api";

        var parts = new List<string>
        {
            $"realm=\"{Escape(realm)}\"",
            $"error=\"invalid_token\""
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            parts.Add($"error_description=\"{Escape(description)}\"");
        }

        if (!string.IsNullOrWhiteSpace(errorUri))
        {
            parts.Add($"error_uri=\"{Escape(errorUri)}\"");
        }

        var wwwAuthenticate = $"Bearer {string.Join(", ", parts)}";

        return new OAuthException(
            "invalid_token",
            StatusCodes.Status401Unauthorized,
            description,
            errorUri,
            wwwAuthenticate);
    }

    public static OAuthException FromInsufficientScope(
        string scope,
        string? description = null,
        string? errorUri = null,
        string? realm = null)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            throw new ArgumentException("Scope is required for insufficient_scope", nameof(scope));
        }

        realm ??= "api";

        var parts = new List<string>
        {
            $"realm=\"{Escape(realm)}\"",
            $"error=\"insufficient_scope\"",
            $"scope=\"{Escape(scope)}\""
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            parts.Add($"error_description=\"{Escape(description)}\"");
        }

        if (!string.IsNullOrWhiteSpace(errorUri))
        {
            parts.Add($"error_uri=\"{Escape(errorUri)}\"");
        }

        var wwwAuthenticate = $"Bearer {string.Join(", ", parts)}";

        return new OAuthException(
            "insufficient_scope",
            StatusCodes.Status403Forbidden,
            description,
            errorUri,
            wwwAuthenticate);
    }

    public string Error { get; }
    public int StatusCode { get; }
    public string? Description { get; }
    public string? ErrorUri { get; }
    public string? WwwAuthenticate { get; }

    public OAuthException(
        string error, 
        int statusCode, 
        string? description = null, 
        string? uri = null, 
        string? wwwAuthenticate = null)
        : base(description ?? error)
    {
        Error = error;
        StatusCode = statusCode;
        Description = description;
        ErrorUri = uri;
        WwwAuthenticate = wwwAuthenticate;
    }

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
