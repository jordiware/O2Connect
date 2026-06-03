using System.Collections.Immutable;

namespace O2Connect.Api.DataFactories.RequestModels;

public sealed record JwtTokenFactoryRequest
{
    public required string ClientId { get; init; }
    public required string Subject { get; init; }
    public required ImmutableHashSet<string> Scopes { get; init; }
    public Dictionary<string, object>? AdditionalClaims { get; init; }
    public string? RefreshToken { get; init; }
}
