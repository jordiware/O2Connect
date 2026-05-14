using O2Connect.Api.Models;
using O2Connect.Api.Repositories;
using O2Connect.Api.Services.Validators;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services.TokenGrantHandlers;

public class ClientCredentialsGrantHandler : TokenGrantHandler
{
    public ClientCredentialsGrantHandler(IEnumerable<IPkceValidator> pkceValidators) : base(pkceValidators)
    {
    }

    public override string GrantType => "client_credentials";

    public override Task<TokenResponse> HandleAsync(TokenRequest request, ValidatedClient client, CancellationToken ct)
    {
        var response = new TokenResponse
        {
            AccessToken = "mock_access_token",
            ExpiresIn = 3600,
            RefreshToken = "mock_refresh_token",
            IdToken = "mock_id_token"
        };

        return Task.FromResult(response);
    }
}
