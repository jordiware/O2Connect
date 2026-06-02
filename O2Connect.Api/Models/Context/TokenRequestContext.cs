using O2Connect.Api.Models.Store;
using O2Connect.Dto.Requests;
using System.Collections.Immutable;

namespace O2Connect.Api.Models.Context;

public sealed class TokenRequestContext
{
    public required TokenRequest TokenRequest { get; init; }
    public required Client Client { get; init; }
    public required GrantType GrantType { get; init; }
    public required ClientAuthenticationMethod ClientAuthenticationMethod { get; init; }
    public required ImmutableHashSet<string> Scopes { get; init; }

    public AuthorizationCode? AuthorizationCode { get; init; } = null;
    public RefreshToken? RefreshToken { get; init; } = null;
}
