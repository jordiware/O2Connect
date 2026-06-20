namespace O2Connect.Api.Models.Store;

public sealed record DeviceAuthorization
{
    public required string DeviceCodeHash { get; init; }
    public required string UserCodeHash { get; init; }

    public required string ClientId { get; init; }
    public required string[] Scopes { get; init; }
    public string? UserId { get; init; }

    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset ExpiresAtUtc { get; init; }

    public DateTimeOffset? AuthorizedAtUtc { get; init; }
    public DateTimeOffset? ConsumedAtUtc { get; init; }
    public DateTimeOffset? DeniedAtUtc { get; init; }

    public required int PollCount { get; init; }
    public required int Interval { get; init; }
    public DateTimeOffset? LastPollAtUtc { get; init; }

    public bool IsAuthorized => AuthorizedAtUtc != null;
    public bool IsConsumed => ConsumedAtUtc != null;
    public bool IsDenied => DeniedAtUtc != null;

    public Client Client { get; init; } = null!;
    public User? User { get; init; }
}
