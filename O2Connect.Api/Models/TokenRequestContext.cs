using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Models;

public sealed record TokenRequestContext
{
    public required TokenRequest TokenRequest { get; init; }
    public required Client Client { get; init; }
    public required GrantType GrantType { get; init; }
    public required ClientAuthenticationMethod ClientAuthenticationMethod { get; init; }
    public required IReadOnlySet<string> Scopes { get; init; }

    public AuthorizationCode? AuthorizationCode { get; init; } = null;
    public RefreshToken? RefreshToken { get; init; } = null;
    public string? UserId { get; init; }
    public DeviceAuthorization? DeviceAuthorization { get; init; }
}
