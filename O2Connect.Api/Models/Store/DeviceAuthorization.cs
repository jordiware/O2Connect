namespace O2Connect.Api.Models.Store;

public sealed record DeviceAuthorization
{
    public required string DeviceCodeHash { get; init; }
    public required string UserCodeHash { get; init; }

    public required string ClientId { get; init; }
    public required string Scope { get; init; }

    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset ExpiresAtUtc { get; init; }

    public DateTimeOffset? AuthorizedAtUtc { get; init; }
    public string? UserId { get; init; }

    public required int PollCount { get; init; }
    public required int Interval { get; init; }
    public DateTimeOffset? LastPollAtUtc { get; init; }

    public required bool IsDenied { get; init; }
    public bool IsAuthorized => AuthorizedAtUtc != null;
}
