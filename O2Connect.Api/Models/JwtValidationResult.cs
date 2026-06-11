namespace O2Connect.Api.Models;

public sealed record JwtValidationResult
{
    public static JwtValidationResult Invalid { get; } = new()
    {
        IsValid = false
    };

    public bool IsValid { get; init; } = true;
    public string TokenType { get; init; } = default!;
    public string? Subject { get; init; }
    public string? ClientId { get; init; }
    public IEnumerable<string> Scopes { get; init; } = [];
    public string? SessionId { get; init; }
    public long? ExpUnix { get; init; }
    public long? IatUnix { get; init; }
    public long? NotBeforeUnix { get; init; }
    public IReadOnlyDictionary<string, string[]> Claims { get; init; } = new Dictionary<string, string[]>();
}
