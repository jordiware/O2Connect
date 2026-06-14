namespace O2Connect.Api.Models.Store;

public sealed record Client
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string NormalizedName { get; init; }
    public required EntityStatus Status { get; init; }
    public required string OwnerId { get; init; }
    public string? PircureUrl { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastModifiedAt { get; init; }
    public DateTimeOffset? RevokedAt { get; init; }
    public string? ClientSecret { get; init; }
    public string? JsonWebKeysUri { get; init; }
    public bool RequiresSecret { get; init; }
    public bool RequiresPkce { get; init; } = true;
    public bool RequiresConsent { get; init; } = true;
    public bool AllowPlainPkce { get; init; } = false;
    public bool AllowPar { get; init; } = true;
    public required IReadOnlySet<string> RedirectUris { get; init; }
    public required IReadOnlySet<string> AllowedGrantTypes { get; init; }
    public required IReadOnlySet<string> AllowedScopes { get; init; }
    public required IReadOnlySet<string> AllowedAuthenticationMethods { get; init; }
    public required IReadOnlySet<string> AllowedResponseTypes { get; init; }
}
