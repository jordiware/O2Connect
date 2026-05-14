using O2Connect.Api.Services.PkceValidators;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services.TokenGrantHandlers;

public class RefreshTokenGrantHandler : TokenGrantHandler
{
    public RefreshTokenGrantHandler(IEnumerable<IPkceValidator> pkceValidators) : base(pkceValidators)
    {
    }

    public override string GrantType => "refresh_token";

    public override Task<TokenResponse> HandleAsync(TokenRequest request, CancellationToken ct)
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
