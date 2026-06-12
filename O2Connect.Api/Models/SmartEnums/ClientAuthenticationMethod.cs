namespace O2Connect.Api.Models.SmartEnums;

public readonly record struct ClientAuthenticationMethod : ISmartEnum<ClientAuthenticationMethod>
{
    private static readonly ClientAuthenticationMethod None = new(string.Empty);

    public static readonly ClientAuthenticationMethod ClientSecretBasic = new("client_secret_basic");
    public static readonly ClientAuthenticationMethod ClientSecretPost = new("client_secret_post");
    public static readonly ClientAuthenticationMethod PrivateKeyJwt = new("private_key_jwt");

    public static IReadOnlyCollection<ClientAuthenticationMethod> Supported { get; } =
    [
        ClientSecretBasic,
        ClientSecretPost,
        PrivateKeyJwt
    ];

    public string Value { get; }

    private ClientAuthenticationMethod(string value)
    {
        Value = value;
    }

    public static implicit operator string(ClientAuthenticationMethod method) => method.Value;

    public static bool TryParse(string? value, out ClientAuthenticationMethod result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        result = value switch
        {
            "client_secret_basic" => ClientSecretBasic,
            "client_secret_post" => ClientSecretPost,
            "private_key_jwt" => PrivateKeyJwt,
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
