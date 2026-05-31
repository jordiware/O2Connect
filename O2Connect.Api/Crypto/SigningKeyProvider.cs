using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Options;
using System.Security.Cryptography;

namespace O2Connect.Api.Crypto;

public interface ISigningKeyProvider
{
    IReadOnlyCollection<SigningKey> GetSigningKeys();
    SigningKey GetActiveKey();
}

public class RsaSigningKeyProvider : ISigningKeyProvider, IDisposable
{
    private readonly JwtOptions _options;

    private readonly object _lock = new();
    private readonly Dictionary<string, SigningKey> _keys = new();

    public RsaSigningKeyProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;

        var rsa = RSA.Create();

        if (!File.Exists("private_key.pem"))
            throw new InvalidOperationException("Private key file missing.");

        rsa.ImportFromPem(File.ReadAllText("private_key.pem"));

        var key = new RsaSecurityKey(rsa)
        {
            KeyId = _options.ActiveKeyId
        };

        var signingKey = new SigningKey
        {
            KeyId = _options.ActiveKeyId,
            Key = key,
            Credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256),
            Status = SigningKeyStatus.Active
        };

        _keys.Add(signingKey.KeyId, signingKey);
    }

    public IReadOnlyCollection<SigningKey> GetSigningKeys()
    {
        lock (_lock)
        {
            return _keys.Values.ToList();
        }
    }

    public SigningKey GetActiveKey()
    {
        lock (_lock)
        {
            return _keys.Values.Single(v => v.Status == SigningKeyStatus.Active);
        }

    }

    public void AddKey(RSA rsa, string keyId)
    {
        if (rsa is null)
            throw new ArgumentNullException(nameof(rsa));

        if (string.IsNullOrWhiteSpace(keyId))
            throw new ArgumentException("KeyId required.", nameof(keyId));

        var securityKey = new RsaSecurityKey(rsa)
        {
            KeyId = keyId
        };

        lock (_lock)
        {
            var newKey = new SigningKey
            {
                KeyId = keyId,
                Key = securityKey,
                Credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256),
                Status = SigningKeyStatus.Retired
            };

            if (!_keys.TryAdd(keyId, newKey))
                throw new InvalidOperationException("Duplicate key id.");
        }
    }

    public void SetActiveKey(string newActiveKeyId)
    {
        lock (_lock)
        {
            if (!_keys.ContainsKey(newActiveKeyId))
                throw new InvalidOperationException("Key not found.");

            if (_keys[newActiveKeyId].Status == SigningKeyStatus.Active)
                return;

            foreach (var (key, value) in _keys)
            {
                if (key == newActiveKeyId)
                {
                    _keys[key] = value.Clone(SigningKeyStatus.Active);
                }
                else if (value.Status == SigningKeyStatus.Active)
                {
                    _keys[key] = value.Clone(SigningKeyStatus.Retired);
                }
            }

            if (_keys.Values.Count(k => k.Status == SigningKeyStatus.Active) != 1)
                throw new InvalidOperationException("Exactly one active key required.");
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var value in _keys.Values)
            {
                value.Key.Rsa.Dispose();
            }
        }
    }
}
