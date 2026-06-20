namespace O2Connect.Api.Models.Store;

public sealed record AuthorizationSession
{
    public required string SessionId { get; init; }
    public required AuthorizationStatus Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }

    public required AuthorizationRequestData Request { get; init; }
    public string? RequestUriCode { get; init; } // PAR reference

    public required string ClientId { get; init; }
    public required string ClientDisplayName { get; init; }

    public string? UserId { get; init; }
    public string? UserDisplayName { get; init; }

    public string[]? RequestedScopes { get; init; }
    public string[]? MissingScopes { get; init; }

    public Client Client { get; init; } = null!;
    public User? User { get; init; }
}

public enum AuthorizationStatus
{
    Initialized,
    RequestStored,

    AuthorizationRequested,

    LoginRequired,
    Authenticated,

    ConsentRequired,
    Consented,

    CodeIssued,

    Cancelled,
    Aborted,
    Expired
}
