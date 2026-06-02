using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using System.Collections.Immutable;

namespace O2Connect.Api.DataFactories.RequestModels;

public sealed record JwtTokenFactoryRequest
{
    public required Client Client { get; init; }
    public required string Subject { get; init; }
    public required ImmutableHashSet<string> Scopes { get; init; }
    public Dictionary<string, object>? AdditionalClaims { get; init; }
    public string? RefreshToken { get; init; }
}
