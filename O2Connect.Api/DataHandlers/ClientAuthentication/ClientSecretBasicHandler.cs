using O2Connect.Api.DataValidators;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Requests;
using System.Text;

namespace O2Connect.Api.DataHandlers.ClientAuthentication;

public class ClientSecretBasicHandler : IClientAuthenticationHandler
{
    private readonly IClientSecretValidator _validator;

    public ClientAuthenticationMethod Method => ClientAuthenticationMethod.ClientSecretBasic;

    public ClientSecretBasicHandler(IClientSecretValidator validator)
    {
        _validator = validator;
    }

    public bool CanAuthenticate(HttpRequest request, TokenRequest tokenRequest)
    {
        var authorizationHeaders = request.Headers.Authorization;

        if (authorizationHeaders.Count == 0)
            return false;

        if (authorizationHeaders.Count > 1)
            throw OAuthException.FromInvalidRequest("Multiple Authorization headers");

        var header = authorizationHeaders.FirstOrDefault();

        return header is not null &&
            header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase) &&
            header.Substring("Basic ".Length).Trim().Length > 0;
    }

    public async Task<string> ExtractClientIdAsync(HttpRequest request, TokenRequest tokenRequest, CancellationToken ct)
    {
        var (clientId, secret) = ExtractCredentialsAsync(request, tokenRequest);

        if (string.IsNullOrWhiteSpace(clientId))
            throw OAuthException.FromInvalidRequest();

        return clientId;
    }

    public async Task<ClientAuthenticationResult> AuthenticateAsync(HttpRequest request, TokenRequest tokenRequest, Client client, CancellationToken ct)
    {
        var (clientId, secret) = ExtractCredentialsAsync(request, tokenRequest);

        if (clientId != client.ClientId)
            throw OAuthException.FromInvalidClient();

        if (string.IsNullOrEmpty(secret))
            throw OAuthException.FromInvalidClient();

        if (!_validator.Validate(client, secret))
            throw OAuthException.FromInvalidClient();

        return ClientAuthenticationResult.Ok(client, Method);
    }

    private (string? clientId, string? secret) ExtractCredentialsAsync(HttpRequest request, TokenRequest tokenRequest)
    {
        var header = request.Headers.Authorization.ToString();
        var encoded = header["Basic ".Length..].Trim();

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var idx = decoded.IndexOf(':');

            if (idx <= 0)
                throw OAuthException.FromInvalidClient();

            var clientId = decoded[..idx];
            var secret = decoded[(idx + 1)..];

            if (secret.Length == 0) 
                secret = null;

            return (clientId, secret);
        }
        catch
        {
            throw OAuthException.FromInvalidClient();
        }
    }
}
