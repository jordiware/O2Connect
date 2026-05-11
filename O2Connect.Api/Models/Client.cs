namespace O2Connect.Api.Models;

public class Client
{
    public string ClientId { get; set; } = default!;
    public string? ClientSecret { get; set; }
    public List<string> RedirectUris { get; set; } = new();
    public List<string> AllowedGrantTypes { get; set; } = new();
    public List<string> AllowedScopes { get; set; } = new();
    public bool RequirePkce { get; set; }
}
