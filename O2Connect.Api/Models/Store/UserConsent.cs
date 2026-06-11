namespace O2Connect.Api.Models.Store;

public sealed record UserConsent
{
    public required string Id { get; init; } = default!;
    public required string UserId { get; init; } = default!;
    public required string ClientId { get; init; } = default!;
    public required IReadOnlySet<string> GrantedScopes { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
