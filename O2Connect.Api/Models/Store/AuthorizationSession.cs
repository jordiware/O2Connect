using O2Connect.Dto.Requests;

namespace O2Connect.Api.Models.Store;

public sealed record AuthorizationSession
{
    public string Id { get; init; } = default!;
    public AuthorizationRequest Request { get; init; } = default!;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public string? UserId { get; init; }
}
