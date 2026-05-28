using O2Connect.Api.Models;
using O2Connect.Api.Models.Context;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.RequestModelValidators;

public interface ITokenRequestValidator
{
    GrantType GrantType { get; }
    Task<TokenRequestContext> ValidateAsync(TokenRequest request,
                                            Client client,
                                            ClientAuthenticationMethod method,
                                            CancellationToken ct);
}
