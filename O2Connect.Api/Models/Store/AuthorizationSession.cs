namespace O2Connect.Api.Models.Store;

public sealed record AuthorizationSession
{
    public required string SessionId { get; init; }

    public required AuthorizationStatus Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }

    // === Request origin ===
    public required AuthorizationRequestData Request { get; init; }
    public string? RequestUriCode { get; init; } // PAR reference

    // === Client ===
    public string? ClientId { get; init; }
    public string? ClientDisplayName { get; init; }

    // === User ===
    public string? UserId { get; init; }
    public string? UserDisplayName { get; init; }

    // === Scopes ===
    public IReadOnlySet<string>? RequestedScopes { get; init; }
    public IReadOnlySet<string>? MissingScopes { get; init; }
}
public enum AuthorizationStatus
{
    // === PAR phase ===
    Initialized,        // PAR created
    RequestStored,      // request_uri issued

    // === Authorization flow ===
    AuthorizationRequested, // /authorize hit (direct or via PAR)

    // === Authentication ===
    LoginRequired,
    Authenticated,

    // === Consent ===
    ConsentRequired,
    Consented,

    // === Finalization ===
    CodeIssued,

    // === Terminal states ===
    Cancelled,
    Aborted,
    Expired
}
