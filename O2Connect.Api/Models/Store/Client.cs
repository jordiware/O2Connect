namespace O2Connect.Api.Models.Store;

public class Client
{
    public string ClientId { get; set; } = default!;
    public string? ClientSecret { get; set; }
    public List<string> RedirectUris { get; set; } = new();
    public List<string> AllowedGrantTypes { get; set; } = new();
    public List<string> AllowedAuthenticationMethods { get; set; } = new();
    public List<string> AllowedScopes { get; set; } = new();
    public bool RequiresSecret { get; set; }
    public bool RequiresPkce { get; set; } = true;
}
