namespace O2Connect.Api.Models.Store;

public sealed record Client
{
    public string ClientId { get; set; } = default!;
    public string ClientName { get; set; } = default!;
    public string? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ClientSecret { get; set; }
    public bool RequiresSecret { get; set; }
    public bool RequiresPkce { get; set; } = true;
    public bool RequiresConsent { get; set; } = true;
    public List<string> RedirectUris { get; set; } = new();
    public List<string> AllowedGrantTypes { get; set; } = new();
    public List<string> AllowedScopes { get; set; } = new();
    public List<string> AllowedAuthenticationMethods { get; set; } = new();
    public string? JsonWebKeysUri { get; set; }
}
