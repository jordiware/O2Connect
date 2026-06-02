using O2Connect.Dto.Requests;
using System.Collections.Immutable;

namespace O2Connect.Api.Models.Store;

public sealed record AuthorizationSession
{
    public string Id { get; init; } = default!;
    public AuthorizationRequest Request { get; init; } = default!;
    public string? ClientDisplayName { get; init; }
    public string? UserId { get; init; }
    public string? UserDisplayName { get; init; }
    public ImmutableHashSet<string> RequestedScopes { get; init; } = [];
    public ImmutableHashSet<string> MissingScopes { get; init; } = [];
    public AuthorizationStage Stage { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
}

public enum AuthorizationStage
{
    Created,
    LoginRequired,
    ConsentRequired,
    Ready,
    Completed,
    Cancelled
}
