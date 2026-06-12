namespace O2Connect.Api.Models.SmartEnums;

public readonly record struct GrantType : ISmartEnum<GrantType>
{
    private static readonly GrantType None = new(string.Empty);

    public static readonly GrantType AuthorizationCode = new("authorization_code");
    public static readonly GrantType ClientCredentials = new("client_credentials");
    public static readonly GrantType RefreshToken = new("refresh_token");
    public static readonly GrantType DeviceCode = new("urn:ietf:params:oauth:grant-type:device_code");

    public static IReadOnlyList<GrantType> Supported { get; } =
    [
        AuthorizationCode,
        ClientCredentials,
        RefreshToken,
        DeviceCode
    ];

    public string Value { get; }

    private GrantType(string value)
    {
        Value = value;
    }

    public static implicit operator string(GrantType type) => type.Value;

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
            "urn:ietf:params:oauth:grant-type:device_code" => DeviceCode,
            _ => None
        };

        return result != None;
    }

    public override string ToString()
    {
        return Value;
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Value);
    }
}
