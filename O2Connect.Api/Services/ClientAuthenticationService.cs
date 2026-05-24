using O2Connect.Api.DataHandlers.ClientAuthentication;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Services;

public interface IClientAuthenticationService
{
    Task<AuthenticatedClient> AuthenticateAsync(HttpRequest request,
                                                TokenRequest tokenRequest,
                                                CancellationToken cancellationToken);
}

public class ClientAuthenticationService : IClientAuthenticationService
{
    private readonly IEnumerable<IClientAuthenticationHandler> _handlers;
    private readonly IClientRepository _clientRepository;

    public ClientAuthenticationService(
        IEnumerable<IClientAuthenticationHandler> handlers,
        IClientRepository clientRepository)
    {
        _handlers = handlers;
        _clientRepository = clientRepository;
    }

    public async Task<AuthenticatedClient> AuthenticateAsync(
        HttpRequest request,
        TokenRequest tokenRequest,
        CancellationToken cancellationToken)
    {
        var matchingHandlers = _handlers
            .Where(h => h.CanHandle(request, tokenRequest))
            .ToList();

        if (matchingHandlers.Count == 0)
            throw OAuthException.FromInvalidClient("No client authentication method provided.");

        if (matchingHandlers.Count > 1)
            throw OAuthException.FromInvalidRequest("Multiple client authentication methods used.");

        var handler = matchingHandlers[0];

        var (clientId, secret) = await handler.ExtractCredentialsAsync(request, tokenRequest);

        var client = await _clientRepository.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            throw OAuthException.FromInvalidClient();

        if (!client.AllowedAuthenticationMethods.Contains(handler.Method.Value))
            throw OAuthException.FromUnauthorizedClient();

        await handler.ValidateAsync(client, secret);

        return new AuthenticatedClient
        {
            ClientId = clientId,
            Client = client,
            AuthenticationMethod = handler.Method
        };
    }
}
