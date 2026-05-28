namespace O2Connect.Api.Models;

public class AudienceSet
{
    private readonly HashSet<string> _audiences;

    public IReadOnlyCollection<string> Values => _audiences;

    public AudienceSet(IEnumerable<string> audiences)
    {
        _audiences = new HashSet<string>(Normalize(audiences), StringComparer.Ordinal);
    }

    public bool IsEmpty => _audiences.Count == 0;

    public bool Contains(string scope) => _audiences.Contains(scope);

    public bool IsSubsetOf(IEnumerable<string> other) =>
        _audiences.IsSubsetOf(other);

    private static IEnumerable<string> Normalize(IEnumerable<string> scopes)
    {
        return scopes.Where(s => !string.IsNullOrWhiteSpace(s))
                     .Select(s => s.Trim());
    }
}
