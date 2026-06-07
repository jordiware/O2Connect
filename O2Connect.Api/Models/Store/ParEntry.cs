namespace O2Connect.Api.Models.Store;

public sealed record ParEntry
{
    public required string RequestUriCode { get; init; }
    public required string ClientId { get; init; }
    public required string RedirectUri { get; init; }
    public required string Scope { get; init; }
    public required string ResponseType { get; init; }

    public required string CodeChallenge { get; init; }
    public required string CodeChallengeMethod { get; init; }

    public string? State { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? ConsumedAt { get; init; }
}