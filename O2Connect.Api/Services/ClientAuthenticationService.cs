using O2Connect.Api.DataHandlers.ClientAuthentication;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Services;

public interface IClientAuthenticationService
{
    Task<ClientAuthenticationResult> AuthenticateAsync(HttpRequest request, TokenRequest tokenRequest, CancellationToken ct);
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

    public async Task<ClientAuthenticationResult> AuthenticateAsync(HttpRequest request, TokenRequest tokenRequest, CancellationToken ct)
    {
        if (request.Headers.Authorization.Any()
            && (!string.IsNullOrWhiteSpace(tokenRequest.ClientId) || !string.IsNullOrWhiteSpace(tokenRequest.ClientSecret)))
        {
            throw OAuthException.FromInvalidRequest("Client credentials must not be provided in both Authorization header and request body.");
        }

        if (!_handlers.Any())
            throw OAuthException.FromServerError("No authentication handlers registered.");

        var matchingHandlers = _handlers.Where(h => h.CanAuthenticate(request, tokenRequest));

        if (matchingHandlers.Count() == 0)
            throw OAuthException.FromInvalidClient();
        if (matchingHandlers.Count() > 1)
            throw OAuthException.FromInvalidRequest("Multiple client authentication methods detected.");

        var authHandler = matchingHandlers.Single();
        var authMethod = authHandler.Method;
        var clientId = await authHandler.ExtractClientIdAsync(request, tokenRequest, ct);
        
        if (string.IsNullOrWhiteSpace(clientId))
            throw OAuthException.FromInvalidClient();

        var client = await _clientRepository.GetByIdAsync(clientId, ct);

        if (client is null)
            throw OAuthException.FromInvalidClient();
        if (!client.AllowedAuthenticationMethods.Select(ClientAuthenticationMethod.Parse).Contains(authMethod))
            throw OAuthException.FromInvalidClient();

        return await authHandler.AuthenticateAsync(request, tokenRequest, client, ct);
    }
}
