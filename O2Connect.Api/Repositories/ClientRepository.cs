using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Repositories;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(string clientId, CancellationToken ct);
    Task<bool> ValidateClientAsync(string clientId, string? clientSecret, CancellationToken ct);
    Task<bool> ValidateRedirectUriAsync(string clientId, string redirectUri, CancellationToken ct);
}

public class InMemoryClientRepository : IClientRepository
{
    private readonly List<Client> _clients = new()
    {
        new Client
        {
            ClientId = "test-client",
            ClientSecret = "secret",
            RedirectUris = ["https://example.com/callback"] ,
            AllowedGrantTypes = ["authorization_code", "client_credentials"],
            AllowedScopes = ["openid", "profile", "api"],
            RequiresSecret = false
        }
    };

    public Task<Client?> GetByIdAsync(string clientId, CancellationToken ct)
    {
        var client = _clients.FirstOrDefault(c => c.ClientId == clientId);
        return Task.FromResult(client);
    }

    public Task<bool> ValidateClientAsync(string clientId, string? clientSecret, CancellationToken ct)
    {
        var client = _clients.FirstOrDefault(c => c.ClientId == clientId);

        if (client == null)
            return Task.FromResult(false);

        // Public client (no secret)
        if (string.IsNullOrEmpty(client.ClientSecret))
            return Task.FromResult(true);

        return Task.FromResult(client.ClientSecret == clientSecret);
    }

    public Task<bool> ValidateRedirectUriAsync(string clientId, string redirectUri, CancellationToken ct)
    {
        var client = _clients.FirstOrDefault(c => c.ClientId == clientId);

        if (client == null)
            return Task.FromResult(false);

        return Task.FromResult(client.RedirectUris.Contains(redirectUri, StringComparer.Ordinal));
    }
}
