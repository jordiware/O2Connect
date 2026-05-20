using O2Connect.Api.Crypto;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.RequestInputs;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;

namespace O2Connect.Api.Services.Authenticators;

public interface IClientAuthenticator
{
    Task<Client> AuthenticateAsync(TokenInput input, CancellationToken ct);
}

public class ClientAuthenticator : IClientAuthenticator
{
    private readonly IClientRepository _repo;
    private readonly ISecretHasher _hasher;

    public ClientAuthenticator(
        IClientRepository repo, 
        ISecretHasher hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task<Client> AuthenticateAsync(TokenInput input, CancellationToken ct)
    {
        var client = await _repo.GetByIdAsync(input.ClientId, ct)
            ?? throw OAuthException.FromInvalidClient();

        if (client.RequiresSecret)
        {
            if (string.IsNullOrWhiteSpace(input.ClientSecret))
                throw OAuthException.FromInvalidClient();

            if (string.IsNullOrWhiteSpace(client.ClientSecret))
                throw OAuthException.FromInvalidClient();

            if (!_hasher.Verify(input.ClientSecret, client.ClientSecret))
                throw OAuthException.FromInvalidClient();
        }

        return client;
    }
}
