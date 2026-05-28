namespace O2Connect.Api.Models;

public class ResourceSet
{
    private readonly HashSet<string> _resources;

    public IReadOnlyCollection<string> Values => _resources;

    public ResourceSet(IEnumerable<string> resources)
    {
        _resources = new HashSet<string>(Normalize(resources), StringComparer.Ordinal);
    }

    public bool IsEmpty => _resources.Count == 0;

    public bool Contains(string scope) => _resources.Contains(scope);

    public bool IsSubsetOf(IEnumerable<string> other) =>
        _resources.IsSubsetOf(other);

    private static IEnumerable<string> Normalize(IEnumerable<string> scopes)
    {
        return scopes.Where(s => !string.IsNullOrWhiteSpace(s))
                     .Select(s => s.Trim());
    }
}
