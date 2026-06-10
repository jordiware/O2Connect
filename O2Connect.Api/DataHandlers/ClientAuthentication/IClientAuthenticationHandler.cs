using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.DataHandlers.ClientAuthentication;

public interface IClientAuthenticationHandler
{
    ClientAuthenticationMethod Method { get; }

    bool CanAuthenticate(HttpRequest request,
                         TokenRequest tokenRequest);

    void ValidateSingleCredentialsSource(HttpRequest request,
                                         TokenRequest tokenRequest);

    Task<string> ExtractClientIdAsync(HttpRequest request,
                                      TokenRequest tokenRequest,
                                      CancellationToken ct);
    
    Task<ClientAuthenticationResult> AuthenticateAsync(HttpRequest request,
                                                       TokenRequest tokenRequest,
                                                       Client client,
                                                       CancellationToken ct);
}
