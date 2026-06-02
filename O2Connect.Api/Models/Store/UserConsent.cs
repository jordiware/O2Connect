using System.Collections.Immutable;

namespace O2Connect.Api.Models.Store;

public sealed record UserConsent
{
    public string Id { get; init; } = default!;
    public string UserId { get; init; } = default!;
    public string ClientId { get; init; } = default!;
    public ImmutableHashSet<string> GrantedScopes { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
