using System.Collections.Immutable;

namespace O2Connect.Api.Models;

public sealed record ConsentEvaluationResult
{
    public bool RequiresConsent { get; init; }
    public ImmutableHashSet<string> MissingScopes { get; init; } = [];
}
