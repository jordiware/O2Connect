namespace O2Connect.Api.Models;

public sealed record ConsentEvaluationResult
{
    public bool RequiresConsent { get; init; }
    public HashSet<string> MissingScopes { get; init; } = new();
}
