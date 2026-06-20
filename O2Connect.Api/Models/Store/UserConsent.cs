namespace O2Connect.Api.Models.Store;

public sealed record UserConsent
{
    public required string UserId { get; init; }
    public required string ClientId { get; init; }

    public required IReadOnlySet<string> GrantedScopes { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }

    public User User { get; init; } = default!;
    public Client Client { get; init; } = default!;
}
