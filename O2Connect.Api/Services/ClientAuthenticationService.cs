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

        var authMethod = DetectAuthenticationMethod(request, tokenRequest);
        var matchingHandlers = _handlers.Where(h => h.Method == authMethod).ToList();

        if (matchingHandlers.Count == 0)
            throw OAuthException.FromServerError("No handler registered for authentication method.");

        if (matchingHandlers.Count > 1)
            throw OAuthException.FromServerError("Multiple handlers registered for the same authentication method.");

        var authHandler = matchingHandlers.Single();
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

    private ClientAuthenticationMethod DetectAuthenticationMethod(HttpRequest request, TokenRequest tokenRequest)
    {
        var methodsUsed = new List<ClientAuthenticationMethod>();

        if (HasBasicAuth(request))
            methodsUsed.Add(ClientAuthenticationMethod.ClientSecretBasic);

        if (!string.IsNullOrWhiteSpace(tokenRequest.ClientId) &&
            !string.IsNullOrWhiteSpace(tokenRequest.ClientSecret))
        {
            methodsUsed.Add(ClientAuthenticationMethod.ClientSecretPost);
        }

        if (!string.IsNullOrWhiteSpace(tokenRequest.ClientAssertion) &&
            !string.IsNullOrWhiteSpace(tokenRequest.ClientAssertionType) &&
            tokenRequest.ClientAssertionType == "urn:ietf:params:oauth:client-assertion-type:jwt-bearer")
        {
            methodsUsed.Add(ClientAuthenticationMethod.PrivateKeyJwt);
        }

        if (methodsUsed.Count == 0)
            throw OAuthException.FromInvalidClient();

        if (methodsUsed.Count > 1)
            throw OAuthException.FromInvalidRequest("Multiple client authentication methods detected.");

        return methodsUsed.Single();
    }

    private bool HasBasicAuth(HttpRequest request)
    {
        var authorizationHeaders = request.Headers.Authorization;

        if (authorizationHeaders.Count > 1)
            throw OAuthException.FromInvalidRequest("Multiple Authorization headers");

        var header = authorizationHeaders.FirstOrDefault();

        return header is not null && 
            header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase) &&
            header.Length > "Basic ".Length;
    }
}
