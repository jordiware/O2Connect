using O2Connect.Api.Models.RequestContexts;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services.TokenGrantHandlers;

public interface ITokenGrantHandler
{
    public GrantType GrantType { get; }
    Task<TokenResponse> HandleAsync(TokenRequestContext context, CancellationToken ct);
}
