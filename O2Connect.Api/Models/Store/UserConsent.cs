namespace O2Connect.Api.Models.Store;

public sealed record UserConsent
{
    public string Id { get; init; } = default!;
    public string UserId { get; init; } = default!;
    public string ClientId { get; init; } = default!;
    public HashSet<string> GrantedScopes { get; init; } = new();
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
