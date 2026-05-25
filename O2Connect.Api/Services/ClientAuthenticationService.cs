using O2Connect.Api.DataHandlers.ClientAuthentication;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Services;

public interface IClientAuthenticationService
{
    Task<Client> AuthenticateAsync(HttpRequest request, TokenRequest tokenRequest, CancellationToken cancellationToken);
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

    public async Task<Client> AuthenticateAsync(HttpRequest request, TokenRequest tokenRequest, CancellationToken cancellationToken)
    {
        string? clientId = null;
        IClientAuthenticationHandler? selectedHandler = null;

        foreach (var handler in _handlers)
        {
            if (!handler.CanHandle(request, tokenRequest))
                continue;

            clientId = await handler.ExtractClientIdAsync(request, tokenRequest);

            if (!string.IsNullOrEmpty(clientId))
            {
                selectedHandler = handler;
                break;
            }
        }

        if (string.IsNullOrEmpty(clientId))
            throw OAuthException.FromInvalidClient();

        var client = await _clientRepository.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            throw OAuthException.FromInvalidClient();

        if (selectedHandler is null || !client.AllowedAuthenticationMethods.Contains(selectedHandler.Method.Value))
            throw OAuthException.FromInvalidClient();

        await selectedHandler.AuthenticateAsync(request, tokenRequest, client);

        return client;
    }
}
