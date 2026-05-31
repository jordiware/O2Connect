namespace O2Connect.Api.Models;

public readonly record struct GrantType : ISmartEnum<GrantType>
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

    public static bool TryParse(string? value, out GrantType result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        result = value switch
        {
            "authorization_code" => AuthorizationCode,
            "client_credentials" => ClientCredentials,
            "refresh_token" => RefreshToken,
            _ => default
        };

        return result != default;
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Value);
    }
}
