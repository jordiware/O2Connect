namespace O2Connect.Api.Models.SmartEnums;

public readonly record struct PkceMethod : ISmartEnum<PkceMethod>
{
    public string Value { get; }

    public static readonly PkceMethod Plain = new("plain");
    public static readonly PkceMethod S256 = new("S256");

    public static IReadOnlyCollection<PkceMethod> Supported { get; } =
    [
        Plain,
        S256
    ];

    private PkceMethod(string value)
    {
        Value = value;
    }

    public static bool TryParse(string? value, out PkceMethod result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        result = value switch
        {
            "plain" => Plain,
            "S256" => S256,
            _ => default
        };

        return result != default;
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Value);
    }
}
