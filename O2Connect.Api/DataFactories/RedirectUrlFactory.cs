namespace O2Connect.Api.DataFactories;

public static class RedirectUrlFactory
{
    public static string Login(string? session = null)
    {
        if (string.IsNullOrWhiteSpace(session))
            return $"/account/login";

        return $"/account/login?session={session}";
    }

    public static string LoginWithReturnUrl(string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return "/account/login";

        return $"/account/login?returnUrl={returnUrl}";
    }

    public static string Consent(string? session = null)
    {
        if (string.IsNullOrWhiteSpace(session))
            return "/consent";

        return $"/consent?session={session}";
    }

    public static string Authorize(string requestUri)
    {
        if (string.IsNullOrWhiteSpace(requestUri))
            return "/connect/authorize";

        return $"/connect/authorize?request_uri={Uri.EscapeDataString(requestUri)}";
    }

    public static string AuthorizeResume(string? session = null, string? requestUri = null)
        => BuildAuthorizeUrl("/connect/authorize/resume", session, requestUri);

    public static string AuthorizeCancel(string? session = null, string? requestUri = null)
        => BuildAuthorizeUrl("/connect/authorize/cancel", session, requestUri);

    public static string AuthorizeInspect(string? session = null, string? requestUri = null)
        => BuildAuthorizeUrl("/connect/authorize/inspect", session, requestUri);

    public static string AuthorizeSuccess(string redirectUri,
                                          string code,
                                          string? state = null)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["code"] = code
        };

        if (!string.IsNullOrWhiteSpace(state))
            queryParams["state"] = state;

        var qs = QueryString.Create(queryParams!).ToString();

        return $"{redirectUri}{qs}";
    }

    public static string AuthorizeError(string redirectUri,
                                        string error,
                                        string? state = null,
                                        string? errorDescription = null,
                                        bool useFragment = false)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["error"] = error
        };

        if (!string.IsNullOrWhiteSpace(state))
            queryParams["state"] = state;

        if (!string.IsNullOrWhiteSpace(errorDescription))
            queryParams["error_description"] = errorDescription;

        var queryString = QueryString.Create(queryParams!).ToString().TrimStart('?', '#');

        if (useFragment)
            return $"{redirectUri}#{queryString}";

        return $"{redirectUri}?{queryString}";
    }

    private static string BuildAuthorizeUrl(string path,
                                            string? session,
                                            string? requestUri)
    {
        var queryParams = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(session))
            queryParams["session"] = session;

        if (!string.IsNullOrWhiteSpace(requestUri))
            queryParams["request_uri"] = requestUri;

        var qs = queryParams.Count > 0
            ? QueryString.Create(queryParams!).ToString()
            : string.Empty;

        return $"{path}{qs}";
    }
}
