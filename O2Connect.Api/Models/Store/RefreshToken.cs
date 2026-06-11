namespace O2Connect.Api.Models.Store;

public sealed record RefreshToken
{
    public required string Token { get; init; }
    public required string SessionId { get; init; }
    public required int Version { get; set; }
    public string? ReplacedByToken { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required string ClientId { get; init; }
    public required string Subject { get; init; }
    public required IReadOnlySet<string> Scopes { get; init; }
    public DateTimeOffset? ConsumedAt { get; set; }
    public bool Consumed { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public bool Revoked { get; set; }
}
