using System.Collections.Immutable;

namespace O2Connect.Api.Models;

public class ValueSet
{
    public ImmutableHashSet<string> Values { get; init; }

    public bool IsEmpty => Values.Count == 0;

    public ValueSet(IEnumerable<string> values)
    {
        Values = new HashSet<string>(values, StringComparer.Ordinal).ToImmutableHashSet();
    }

    public bool Contains(string value) => Values.Contains(value, StringComparer.Ordinal);

    public bool IsSubsetOf(IEnumerable<string> other) => Values.IsSubsetOf(other);

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
