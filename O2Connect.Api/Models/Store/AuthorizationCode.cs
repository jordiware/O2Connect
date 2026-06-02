using System.Collections.Immutable;

namespace O2Connect.Api.Models.Store;

public sealed record AuthorizationCode
{
    public string Code { get; init; } = default!;
    public string ClientId { get; init; } = default!;
    public string UserId { get; init; } = default!;
    public string RedirectUri { get; init; } = default!;
    public string CodeChallenge { get; init; } = default!;
    public string? CodeChallengeMethod { get; init; }
    public ImmutableHashSet<string> Scopes { get; init; } = [];
    public DateTimeOffset ExpiresAt { get; init; }
    public string SubjectId { get; init; } = default!;
    public string? Nonce { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public bool Consumed { get; init; } = false;
}
