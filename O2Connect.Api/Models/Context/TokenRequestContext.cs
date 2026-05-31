using O2Connect.Api.Models.Store;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Models.Context;

public sealed class TokenRequestContext
{
    public required TokenRequest TokenRequest { get; init; }
    public required Client Client { get; init; }
    public required AuthorizationCode AuthorizationCode { get; init; }
    public required GrantType GrantType { get; init; }
    public required ClientAuthenticationMethod ClientAuthenticationMethod { get; init; }
}
