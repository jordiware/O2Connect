using O2Connect.Api.Exceptions;

namespace O2Connect.Api.Models;

public readonly record struct PkceMethod(string Value)
{
    public static readonly PkceMethod Plain = new("plain");
    public static readonly PkceMethod S256 = new("S256");

    public static PkceMethod Parse(string value)
    {
        return value switch
        {
            "plain" => Plain,
            "S256" => S256,
            _ => throw OAuthException.FromInvalidRequest()
        };
    }
}
