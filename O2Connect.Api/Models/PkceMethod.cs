using O2Connect.Api.Exceptions;

namespace O2Connect.Api.Models;

public readonly record struct PkceMethod
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

    public static PkceMethod Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw OAuthException.FromInvalidRequest();

        return value switch
        {
            "plain" => Plain,
            "S256" => S256,
            _ => throw OAuthException.FromInvalidRequest()
        };
    }
}
