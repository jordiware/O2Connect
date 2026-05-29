using Microsoft.Extensions.Primitives;
using O2Connect.Api.DataHandlers.ClientAuthentication;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using System.Text;

namespace O2Connect.Api.Services;

public interface IClientAuthenticationService
{
    Task<ClientAuthenticationResult> AuthenticateAsync(HttpRequest request, TokenRequest tokenRequest, CancellationToken cancellationToken);
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

    public async Task<ClientAuthenticationResult> AuthenticateAsync(HttpRequest request, TokenRequest tokenRequest, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization.Any()
            && (!string.IsNullOrWhiteSpace(tokenRequest.ClientId) || !string.IsNullOrWhiteSpace(tokenRequest.ClientSecret)))
        {
            throw OAuthException.FromInvalidRequest("Client credentials must not be provided in both Authorization header and request body.");
        }

        var (clientId, clientSecret) = GetClientCredentials(tokenRequest, request.Headers.Authorization);
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

        return await selectedHandler.AuthenticateAsync(request, tokenRequest, client);
    }

    private (string? clientId, string? clientSecret) GetClientCredentials(TokenRequest request, StringValues authorizationHeaders)
    {
        if (authorizationHeaders.Count > 1)
            throw OAuthException.FromInvalidRequest("Multiple Authorization headers");

        var header = authorizationHeaders.FirstOrDefault();

        if (header?.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase) == true)
        {
            var encoded = header["Basic ".Length..].Trim();

            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var separatorIndex = decoded.IndexOf(':');

                if (separatorIndex <= 0)
                    throw OAuthException.FromInvalidClient();

                var clientId = decoded[..separatorIndex];
                var clientSecret = decoded[(separatorIndex + 1)..];

                return (clientId, clientSecret);
            }
            catch
            {
                throw OAuthException.FromInvalidClient();
            }
        }

        return (request.ClientId, request.ClientSecret);
    }
}
