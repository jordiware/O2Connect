namespace O2Connect.Api.Models.Store;

public sealed record Client
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string NormalizedName { get; init; }
    public string? ImageUrl { get; init; }
    public required EntityStatus Status { get; init; }
    public required string OwnerId { get; init; }
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
    public required string[] RedirectUris { get; init; }
    public required string[] AllowedGrantTypes { get; init; }
    public required string[] AllowedScopes { get; init; }
    public required string[] AllowedAuthenticationMethods { get; init; }
    public required string[] AllowedResponseTypes { get; init; }

    public User Owner { get; init; } = default!;
    public ICollection<UserConsent> Consents { get; set; } = new List<UserConsent>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<AuthorizationCode> AuthorizationCodes { get; set; } = new List<AuthorizationCode>();
    public ICollection<ParEntry> ParEntries { get; set; } = new List<ParEntry>();
    public ICollection<DeviceAuthorization> DeviceAuthorizations { get; set; } = new List<DeviceAuthorization>();
    public ICollection<AuthorizationSession> AuthorizationSessions { get; set; } = new List<AuthorizationSession>();
}
