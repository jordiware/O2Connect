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

    public bool CanAuthenticate(HttpRequest request, TokenRequest tokenRequest)
    {
        return !string.IsNullOrWhiteSpace(tokenRequest.ClientId) &&
               !string.IsNullOrWhiteSpace(tokenRequest.ClientSecret);
    }

    public void ValidateSingleCredentialsSource(HttpRequest request, TokenRequest tokenRequest)
    {
        if (!string.IsNullOrWhiteSpace(tokenRequest.ClientAssertionType)
            || !string.IsNullOrWhiteSpace(tokenRequest.ClientAssertion)
            || request.Headers.Authorization.Count != 0)
            throw OAuthException.FromInvalidRequest("Multiple Authorization sources");
    }

    public Task<string> ExtractClientIdAsync(HttpRequest request, TokenRequest tokenRequest, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tokenRequest.ClientId))
            throw OAuthException.FromInvalidRequest();

        return Task.FromResult(tokenRequest.ClientId);
    }

    public Task<ClientAuthenticationResult> AuthenticateAsync(HttpRequest request, TokenRequest tokenRequest, Client client, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tokenRequest.ClientId))
            throw OAuthException.FromInvalidRequest();

        if (string.IsNullOrWhiteSpace(tokenRequest.ClientSecret))
            throw OAuthException.FromInvalidRequest();

        if (!_validator.Validate(client, tokenRequest.ClientSecret))
            throw OAuthException.FromInvalidClient();

        return Task.FromResult(ClientAuthenticationResult.Ok(client, Method));
    }
}
