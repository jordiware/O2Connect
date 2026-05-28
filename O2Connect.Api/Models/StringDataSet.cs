namespace O2Connect.Api.Models;

public static class StringDataSet
{
    public static IEnumerable<string> Split(string? data, char separator)
    {
        var dataSet = string.IsNullOrWhiteSpace(data) ?
                      Array.Empty<string>() :
                      data.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                          .Distinct(StringComparer.Ordinal)
                          .ToArray();

        return dataSet.Where(s => !string.IsNullOrWhiteSpace(s))
                      .Select(s => s.Trim());
    }
}
