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

    public Task<(string clientId, string? secret)> ExtractCredentialsAsync(HttpRequest request, TokenRequest tokenRequest)
    {
        return Task.FromResult((tokenRequest.ClientId!, tokenRequest.ClientSecret));
    }

    public Task ValidateAsync(Client client, string? secret)
    {
        if (string.IsNullOrEmpty(secret))
            throw OAuthException.FromInvalidClient();

        if (!_validator.Validate(client, secret))
            throw OAuthException.FromInvalidClient();

        return Task.CompletedTask;
    }
}
