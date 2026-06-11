namespace O2Connect.Api.Models;

public sealed class JsonWebKey
{
    public string kty { get; init; } = "RSA";
    public string use { get; init; } = "sig";
    public string kid { get; init; } = default!;
    public string alg { get; init; } = default!;
    public string n { get; init; } = default!;
    public string e { get; init; } = default!;
}
