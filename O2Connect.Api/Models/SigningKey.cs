using Microsoft.IdentityModel.Tokens;

namespace O2Connect.Api.Models;

public sealed class SigningKey
{
    public required string KeyId { get; init; }
    public required RsaSecurityKey Key { get; init; }
    public required SigningCredentials Credentials { get; init; }
    public required SigningKeyStatus Status { get; init; }

    public SigningKey Clone(SigningKeyStatus status)
    {
        return new SigningKey
        {
            KeyId = KeyId,
            Key = Key,
            Credentials = Credentials,
            Status = status
        };
    }

    public JsonWebKey ToJwk()
    {
        var (modulus, exponent) = GetJwkRsaMaterial();

        return new JsonWebKey
        {
            kty = "RSA",
            use = "sig",
            kid = KeyId,
            alg = Credentials.Algorithm,
            n = Base64UrlEncode(modulus),
            e = Base64UrlEncode(exponent)
        };
    }

    private (byte[] modulus, byte[] exponent) GetJwkRsaMaterial()
    {
        var rsa = Key.Rsa ?? throw new InvalidOperationException("RSA key instance is missing.");
        var parameters = rsa.ExportParameters(false);

        if (parameters.Modulus is null || parameters.Exponent is null)
            throw new InvalidOperationException("Active RSA key is not valid or exportable.");

        return (parameters.Modulus, parameters.Exponent);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
