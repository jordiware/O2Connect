namespace O2Connect.Api.Models;

public class ValueSet
{
    private readonly HashSet<string> _values;

    public IReadOnlyCollection<string> Values => _values;

    public bool IsEmpty => _values.Count == 0;

    public ValueSet(IEnumerable<string> values)
    {
        _values = new HashSet<string>(values, StringComparer.Ordinal);
    }

    public bool Contains(string value) => _values.Contains(value);

    public bool IsSubsetOf(IEnumerable<string> other) =>
        _values.IsSubsetOf(other);

    public static ValueSet FromDataString(string? data, char separator)
    {
        var values = string.IsNullOrWhiteSpace(data) ?
                     Array.Empty<string>() :
                     data.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => s.Trim())
                         .Where(s => !string.IsNullOrWhiteSpace(s))
                         .Distinct(StringComparer.Ordinal)
                         .ToArray();

        return new ValueSet(values);
    }
}
