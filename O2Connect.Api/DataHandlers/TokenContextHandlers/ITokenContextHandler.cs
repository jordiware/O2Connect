using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Dto.OidcOAuth.Connect;

namespace O2Connect.Api.DataHandlers.TokenContextHandlers;

public interface ITokenContextHandler
{
    public GrantType GrantType { get; }
    Task<TokenResponse> HandleAsync(TokenRequestContext context, CancellationToken ct);
}
