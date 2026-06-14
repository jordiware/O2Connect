using O2Connect.Api.DataValidators;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
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

        if (authorizationHeaders.Count != 1)
            return false;

        var header = authorizationHeaders[0];

        return header is not null
               && header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase)
               && header.AsSpan("Basic ".Length).Trim().Length > 0;
    }

    public void ValidateSingleCredentialsSource(HttpRequest request, TokenRequest tokenRequest)
    {
        if (request.Headers.Authorization.Count != 1)
            throw OAuthException.FromInvalidRequest("Multiple Authorization headers");

        if (!string.IsNullOrWhiteSpace(tokenRequest.ClientId)
            || !string.IsNullOrWhiteSpace(tokenRequest.ClientSecret)
            || !string.IsNullOrWhiteSpace(tokenRequest.ClientAssertionType)
            || !string.IsNullOrWhiteSpace(tokenRequest.ClientAssertion))
            throw OAuthException.FromInvalidRequest("Multiple Authorization sources");
    }

    public Task<string> ExtractClientIdAsync(HttpRequest request, TokenRequest tokenRequest, CancellationToken ct)
    {
        var (clientId, secret) = ExtractCredentials(request, tokenRequest);

        if (string.IsNullOrWhiteSpace(clientId))
            throw OAuthException.FromInvalidRequest();

        return Task.FromResult(clientId);
    }

    public Task<ClientAuthenticationResult> AuthenticateAsync(HttpRequest request, TokenRequest tokenRequest, Client client, CancellationToken ct)
    {
        var (clientId, secret) = ExtractCredentials(request, tokenRequest);

        if (clientId != client.Id)
            return Task.FromResult(ClientAuthenticationResult.Fail());

        if (string.IsNullOrEmpty(secret))
            return Task.FromResult(ClientAuthenticationResult.Fail());

        if (!_validator.Validate(client, secret))
            return Task.FromResult(ClientAuthenticationResult.Fail());

        return Task.FromResult(ClientAuthenticationResult.Ok(client, Method));
    }

    private (string? clientId, string? secret) ExtractCredentials(HttpRequest request, TokenRequest tokenRequest)
    {
        var header = request.Headers.Authorization[0];

        if (header is not null && !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            throw OAuthException.FromInvalidRequest("Invalid Authorization scheme");

        var encoded = header.AsSpan("Basic ".Length).Trim();

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded.ToString()));
            var idx = decoded.IndexOf(':');

            if (idx <= 0)
                throw OAuthException.FromInvalidClient();

            var clientId = decoded[..idx];
            var secret = decoded[(idx + 1)..];

            if (secret.Length == 0) 
                secret = null;

            return (clientId, secret);
        }
        catch (FormatException)
        {
            throw OAuthException.FromInvalidClient();
        }
        catch (DecoderFallbackException)
        {
            throw OAuthException.FromInvalidClient();
        }
    }
}
