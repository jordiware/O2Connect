namespace O2Connect.Api.Models.Store;

public class AuthorizationCode
{
    public string Code { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string RedirectUri { get; set; } = default!;
    public string? CodeChallenge { get; set; }
    public string? CodeChallengeMethod { get; set; }
    public required ScopeSet Scopes { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string SubjectId { get; set; } = default!;
    public string? Nonce { get; set; }
    public DateTime CreatedAt { get; set; }
}
