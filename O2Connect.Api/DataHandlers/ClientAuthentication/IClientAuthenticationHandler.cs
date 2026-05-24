using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.DataHandlers.ClientAuthentication;

public interface IClientAuthenticationHandler
{
    ClientAuthenticationMethod Method { get; }

    bool CanHandle(HttpRequest request, TokenRequest tokenRequest);

    Task<(string clientId, string? secret)> ExtractCredentialsAsync(HttpRequest request, TokenRequest tokenRequest);

    Task ValidateAsync(Client client, string? secret);
}
