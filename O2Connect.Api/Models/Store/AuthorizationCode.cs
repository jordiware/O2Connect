namespace O2Connect.Api.Models.Store;

public record AuthorizationCode
{
    public string Code { get; init; } = default!;
    public string ClientId { get; init; } = default!;
    public string RedirectUri { get; init; } = default!;
    public byte[] CodeChallenge { get; init; } = [];
    public string? CodeChallengeMethod { get; init; }
    public required ValueSet Scopes { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string SubjectId { get; init; } = default!;
    public string? Nonce { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool Consumed { get; init; } = false;
}
