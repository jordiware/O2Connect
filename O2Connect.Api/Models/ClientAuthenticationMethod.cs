using O2Connect.Api.Exceptions;

namespace O2Connect.Api.Models;

public readonly record struct ClientAuthenticationMethod
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

    public static ClientAuthenticationMethod Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw OAuthException.FromInvalidRequest();

        return value switch
        {
            "client_secret_basic" => ClientSecretBasic,
            "client_secret_post" => ClientSecretPost,
            "private_key_jwt" => PrivateKeyJwt,
            _ => throw OAuthException.FromInvalidRequest()
        };
    }
}
