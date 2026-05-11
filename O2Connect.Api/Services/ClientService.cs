using O2Connect.Api.Models;

namespace O2Connect.Api.Services;

public interface IClientService
{
    Task<Client?> GetByIdAsync(string clientId);
    Task<bool> ValidateClientAsync(string clientId, string? clientSecret);
    Task<bool> ValidateRedirectUriAsync(string clientId, string redirectUri);
}

public class InMemoryClientService : IClientService
{
    private readonly List<Client> _clients = new()
    {
        new Client
        {
            ClientId = "test-client",
            ClientSecret = "secret",
            RedirectUris = new List<string> { "https://example.com/callback" },
            AllowedGrantTypes = new List<string> { "authorization_code", "client_credentials" },
            AllowedScopes = new List<string> { "openid", "profile", "api" },
            RequirePkce = false
        }
    };

    public Task<Client?> GetByIdAsync(string clientId)
    {
        var client = _clients.FirstOrDefault(c => c.ClientId == clientId);
        return Task.FromResult(client);
    }

    public Task<bool> ValidateClientAsync(string clientId, string? clientSecret)
    {
        var client = _clients.FirstOrDefault(c => c.ClientId == clientId);

        if (client == null)
            return Task.FromResult(false);

        // Public client (no secret)
        if (string.IsNullOrEmpty(client.ClientSecret))
            return Task.FromResult(true);

        return Task.FromResult(client.ClientSecret == clientSecret);
    }

    public Task<bool> ValidateRedirectUriAsync(string clientId, string redirectUri)
    {
        var client = _clients.FirstOrDefault(c => c.ClientId == clientId);

        if (client == null)
            return Task.FromResult(false);

        return Task.FromResult(client.RedirectUris.Contains(redirectUri, StringComparer.Ordinal));
    }
}
