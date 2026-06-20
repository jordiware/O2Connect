namespace O2Connect.Api.Models.Store;

public sealed record AuthorizationCode
{
    public required string Code { get; init; }

    public required string ClientId { get; init; }
    public required string UserId { get; init; }

    public required string RedirectUri { get; init; }

    public required string CodeChallenge { get; init; }
    public required string CodeChallengeMethod { get; init; }

    public required string[] Scopes { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }

    public string? SubjectId { get; init; }
    public string? Nonce { get; init; }

    public bool IsConsumed { get; init; } = false;

    public Client Client { get; init; } = null!;
    public User User { get; init; } = null!;
}
