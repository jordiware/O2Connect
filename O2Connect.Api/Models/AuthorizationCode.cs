namespace O2Connect.Api.Models;

public class AuthorizationCode
{
    public string Code { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string RedirectUri { get; set; } = default!;
    public string? CodeChallenge { get; set; }
    public string? CodeChallengeMethod { get; set; }
    public string? Scope { get; set; }
    public DateTime ExpiresAt { get; set; }
}

