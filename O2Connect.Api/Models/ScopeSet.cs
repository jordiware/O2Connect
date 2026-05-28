namespace O2Connect.Api.Models;

public sealed class ScopeSet
{
    private readonly HashSet<string> _scopes;

    public IReadOnlyCollection<string> Values => _scopes;

    public ScopeSet(IEnumerable<string> scopes)
    {
        _scopes = new HashSet<string>(Normalize(scopes), StringComparer.Ordinal);
    }

    public bool IsEmpty => _scopes.Count == 0;

    public bool Contains(string scope) => _scopes.Contains(scope);

    public bool IsSubsetOf(IEnumerable<string> other) =>
        _scopes.IsSubsetOf(other);

    private static IEnumerable<string> Normalize(IEnumerable<string> scopes)
    {
        return scopes.Where(s => !string.IsNullOrWhiteSpace(s))
                     .Select(s => s.Trim());
    }
}
