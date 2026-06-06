namespace O2Connect.Api.Models.Store;

public sealed record ParAuthorizationSession
{
    public string? Code { get; init; }
    public required string SessionId { get; init; }
    public required string ClientId { get; init; }
    public string? UserId { get; init; }
    public required string RedirectUri { get; init; }
    public required string RequestUri { get; init; }
    public required string Scope { get; init; }
    public required string CodeChallenge { get; init; }
    public required string CodeChallengeMethod { get; init; }
    public string? State { get; init; }
    public required ParAuthStatus Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
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
