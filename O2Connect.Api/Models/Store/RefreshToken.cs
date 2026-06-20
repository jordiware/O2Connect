namespace O2Connect.Api.Models.Store;

public sealed record RefreshToken
{
    public required string Token { get; init; }
    public required string SessionId { get; init; }

    public required int Version { get; init; }
    
    public string? ReplacedByToken { get; init; }
    
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    
    public required string ClientId { get; init; }
    public required string Subject { get; init; }
    
    public required string[] Scopes { get; init; }
    
    public DateTimeOffset? ConsumedAt { get; init; }
    public bool IsConsumed => ConsumedAt != null;
    
    public DateTimeOffset? RevokedAt { get; init; }
    public bool IsRevoked => RevokedAt != null;

    public Client Client { get; init; } = default!;
    public User User { get; init; } = default!;
    public RefreshToken? ReplacedBy { get; init; }
}
