namespace O2Connect.Api.Models.Store;

public sealed record ParAuthorizationSession
{
    public required string SessionId { get; init; }

    public required string ClientId { get; init; }
    public required string RedirectUri { get; init; }
    public required string Scope { get; init; }

    public required string CodeChallenge { get; init; }
    public required string CodeChallengeMethod { get; init; }

    public required ParAuthState State { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}

public enum ParAuthState
{
    Initialized,
    Authenticated,
    Consented,
    Denied,
    CodeIssued,
    Aborted
}
