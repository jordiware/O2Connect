using O2Connect.Dto.Requests;

namespace O2Connect.Api.Models.Store;

public sealed record AuthorizationSession
{
    public required string SessionId { get; init; }
    public required AuthorizationStage Stage { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required AuthorizationRequest Request { get; init; }
    public string? ClientId { get; init; }
    public string? ClientDisplayName { get; init; }
    public string? UserId { get; init; }
    public string? UserDisplayName { get; init; }
    public IReadOnlySet<string>? RequestedScopes { get; init; }
    public IReadOnlySet<string>? MissingScopes { get; init; }
}

public enum AuthorizationStage
{
    Created,
    LoginRequired,
    LoggedIn,
    ConsentRequired,
    ConsentGranted,
    Cancelled
}
