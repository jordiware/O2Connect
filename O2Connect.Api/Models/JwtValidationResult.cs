namespace O2Connect.Api.Models;

public sealed record JwtValidationResult
{
    public bool IsValid { get; init; }
    public string Subject { get; init; } = default!;
    public string ClientId { get; init; } = default!;
    public IEnumerable<string> Scopes { get; init; } = default!;
    public string? SessionId { get; init; } = default!;
    public long? ExpUnix { get; init; }
    public long? IatUnix { get; init; }
    public IDictionary<string, object> Claims { get; init; } = new Dictionary<string, object>();
}
