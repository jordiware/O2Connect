namespace O2Connect.Api.Models;

public class ValueSet
{
    public IReadOnlySet<string> Values { get; init; }

    public bool IsEmpty => Values.Count == 0;

    public ValueSet(ISet<string> values)
    {
        Values = new HashSet<string>(values, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToHashSet();
    }

    public bool Contains(string value) => Values.Contains(value, StringComparer.Ordinal);

    public bool IsSubsetOf(IEnumerable<string> other) => Values.IsSubsetOf(other);

    public string ToString(char separator) => string.Join(separator, Values);

    public static ValueSet FromDataString(string? data, char separator)
    {
        var values = string.IsNullOrWhiteSpace(data) ?
                     [] :
                     data.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => s.Trim())
                         .Where(s => !string.IsNullOrWhiteSpace(s))
                         .Distinct(StringComparer.Ordinal)
                         .ToHashSet();

        return new ValueSet(values);
    }
}
