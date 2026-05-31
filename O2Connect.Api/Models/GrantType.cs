using O2Connect.Api.Exceptions;

namespace O2Connect.Api.Models;

public readonly record struct GrantType
{
    public string Value { get; }

    public static readonly GrantType AuthorizationCode = new("authorization_code");
    public static readonly GrantType ClientCredentials = new("client_credentials");
    public static readonly GrantType RefreshToken = new("refresh_token");

    public static IReadOnlyCollection<GrantType> Supported { get; } =
    [
        AuthorizationCode,
        ClientCredentials,
        RefreshToken
    ];

    private GrantType(string value)
    {
        Value = value;
    }

    public static GrantType Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw OAuthException.FromInvalidRequest("Missing grant_type.");

        return value switch
        {
            "authorization_code" => AuthorizationCode,
            "client_credentials" => ClientCredentials,
            "refresh_token" => RefreshToken,
            _ => throw OAuthException.FromUnsupportedGrantType(),
        };
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Value);
    }
}
