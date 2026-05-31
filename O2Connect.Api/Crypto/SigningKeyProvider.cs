using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Options;
using System.Security.Cryptography;

namespace O2Connect.Api.Crypto;

public interface ISigningKeyProvider
{
    SigningKey GetActiveKey();
}

public class RsaSigningKeyProvider : ISigningKeyProvider
{
    private readonly JwtOptions _options;
    private readonly RSA _rsa;

    public RsaSigningKeyProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;

        _rsa = RSA.Create();

        // In production: load from secure storage (cert, vault, HSM)
        _rsa.ImportFromPem(File.ReadAllText("private_key.pem"));
    }

    public SigningKey GetActiveKey()
    {
        var securityKey = new RsaSecurityKey(_rsa)
        {
            KeyId = _options.ActiveKeyId
        };

        return new SigningKey
        {
            KeyId = _options.ActiveKeyId,
            Key = securityKey,
            Credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256)
        };
    }
}
