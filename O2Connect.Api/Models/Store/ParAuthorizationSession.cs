namespace O2Connect.Api.Models.Store;

public sealed record ParAuthorizationSession
{
    public required string SessionId { get; init; }
    public required ParAuthStatus Stage { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required string RequestUriCode { get; init; }
    public string? UserId { get; init; }
}

public enum ParAuthStatus
{
    Initialized,
    AwaitingLogin,
    Authenticated,
    AwaitingConsent,
    Consented,
    CodeIssued,
    Aborted,
    Expired
}
