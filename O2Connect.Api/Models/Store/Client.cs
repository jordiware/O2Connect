using System.Collections.Immutable;

namespace O2Connect.Api.Models.Store;

public sealed record Client
{
    public string ClientId { get; set; } = default!;
    public string ClientName { get; set; } = default!;
    public string? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ClientSecret { get; set; }
    public bool IsActive { get; set; }
    public bool RequiresSecret { get; set; }
    public bool RequiresPkce { get; set; } = true;
    public bool RequiresConsent { get; set; } = true;
    public ImmutableHashSet<string> RedirectUris { get; set; } = [];
    public ImmutableHashSet<string> AllowedGrantTypes { get; set; } = [];
    public ImmutableHashSet<string> AllowedScopes { get; set; } = [];
    public ImmutableHashSet<string> AllowedAuthenticationMethods { get; set; } = [];
    public string? JsonWebKeysUri { get; set; }
}
