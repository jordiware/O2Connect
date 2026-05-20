using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Models.FactoryRequests;

public sealed class TokenFactoryRequest
{
    public required Client Client { get; init; }

    public required string Subject { get; init; }

    public required IReadOnlyCollection<string> Scopes { get; init; }

    public Dictionary<string, object>? AdditionalClaims { get; init; }
}
