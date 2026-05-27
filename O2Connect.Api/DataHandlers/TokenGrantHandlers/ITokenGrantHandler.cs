using O2Connect.Api.Models;
using O2Connect.Api.Models.Context;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.DataHandlers.TokenGrantHandlers;

public interface ITokenGrantHandler
{
    public GrantType GrantType { get; }
    Task<TokenResponse> HandleAsync(TokenRequestContext context, CancellationToken ct);
}
