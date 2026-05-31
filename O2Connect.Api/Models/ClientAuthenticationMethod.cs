namespace O2Connect.Api.Models;

public readonly record struct ClientAuthenticationMethod : ISmartEnum<ClientAuthenticationMethod>
{
    public string Value { get; }

    public static readonly ClientAuthenticationMethod ClientSecretBasic = new("client_secret_basic");
    public static readonly ClientAuthenticationMethod ClientSecretPost = new("client_secret_post");
    public static readonly ClientAuthenticationMethod PrivateKeyJwt = new("private_key_jwt");

    public static IReadOnlyCollection<ClientAuthenticationMethod> Supported { get; } =
    [
        ClientSecretBasic,
        ClientSecretPost,
        PrivateKeyJwt
    ];

    private ClientAuthenticationMethod(string value)
    {
        Value = value;
    }

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
            _ => default
        };

        return result != default;
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Value);
    }
}
