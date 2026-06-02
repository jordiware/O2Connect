namespace O2Connect.Dto.Requests;

public sealed record ConsentDecisionRequest
{
    public bool Approved { get; init; }
    public HashSet<string> ApprovedScopes { get; init; } = [];
}
