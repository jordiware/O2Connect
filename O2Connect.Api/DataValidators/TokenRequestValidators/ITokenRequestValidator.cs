using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.OidcOAuth.Connect;

namespace O2Connect.Api.DataValidators.TokenRequestValidators;

public interface ITokenRequestValidator
{
    GrantType GrantType { get; }
    Task<TokenRequestContext> ValidateAsync(TokenRequest request,
                                            Client client,
                                            ClientAuthenticationMethod method,
                                            CancellationToken ct);
}
