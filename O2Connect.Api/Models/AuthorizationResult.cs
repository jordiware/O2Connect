namespace O2Connect.Api.Models;

public sealed record AuthorizationResult
{
    public bool Success { get; init; }
    public AuthorizationResultResponseMode ResponseMode { get; init; } = AuthorizationResultResponseMode.Query;
    public bool IsRedirect { get; init; }
    public string? RedirectUri { get; init; } = default!;

    public string? Code { get; init; }
    public string? State { get; init; }

    public string? Error { get; init; }
    public string? ErrorDescription { get; init; }
}

public enum AuthorizationResultResponseMode
{
    Query,
    Fragment,
    FormPost
}
