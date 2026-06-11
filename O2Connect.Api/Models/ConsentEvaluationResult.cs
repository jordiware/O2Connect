namespace O2Connect.Api.Models;

public sealed record ConsentEvaluationResult
{
    public bool RequiresConsent { get; init; }
    public IReadOnlySet<string>? MissingScopes { get; init; }
}
