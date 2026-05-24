using O2Connect.Api.Exceptions;

namespace O2Connect.Api.Models;

public readonly record struct ClientAuthenticationMethod(string Value)
{
    public static readonly ClientAuthenticationMethod ClientSecretBasic = new("client_secret_basic");
    public static readonly ClientAuthenticationMethod ClientSecretPost = new("client_secret_post");
    public static readonly ClientAuthenticationMethod PrivateKeyJwt = new("private_key_jwt");

    public static ClientAuthenticationMethod Parse(string value)
    {
        return value switch
        {
            "client_secret_basic" => ClientSecretBasic,
            "client_secret_post" => ClientSecretPost,
            "private_key_jwt" => PrivateKeyJwt,
            _ => throw OAuthException.FromInvalidRequest()
        };
    }
}
