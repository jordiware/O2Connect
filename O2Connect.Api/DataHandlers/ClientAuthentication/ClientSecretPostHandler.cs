using O2Connect.Api.DataValidators;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.DataHandlers.ClientAuthentication;

public class ClientSecretPostHandler : IClientAuthenticationHandler
{
    private readonly IClientSecretValidator _validator;

    public ClientAuthenticationMethod Method => ClientAuthenticationMethod.ClientSecretPost;

    public ClientSecretPostHandler(IClientSecretValidator validator)
    {
        _validator = validator;
    }

    public bool CanHandle(HttpRequest request, TokenRequest tokenRequest)
        => !string.IsNullOrEmpty(tokenRequest.ClientId);

    public Task<string?> ExtractClientIdAsync(HttpRequest request, TokenRequest tokenRequest)
    {
        return Task.FromResult(tokenRequest.ClientId);
    }

    public async Task<ClientAuthenticationResult> AuthenticateAsync(HttpRequest request, TokenRequest tokenRequest, Client client)
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

    public (string clientId, string? secret) ExtractCredentialsAsync(HttpRequest request, TokenRequest tokenRequest)
    {
        return (tokenRequest.ClientId!, tokenRequest.ClientSecret);
    }
}
