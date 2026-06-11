namespace O2Connect.Api.Models.Store;

public sealed record DeviceAuthorization
{
    public required string DeviceCodeHash { get; init; }
    public required string UserCodeHash { get; init; }

    public required string ClientId { get; init; }
    public required string Scope { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }

    public DateTime? AuthorizedAtUtc { get; init; }
    public string? UserId { get; init; }

    public required int PollCount { get; init; }
    public DateTime? LastPollAtUtc { get; init; }

    public bool IsAuthorized => AuthorizedAtUtc != null;
}
