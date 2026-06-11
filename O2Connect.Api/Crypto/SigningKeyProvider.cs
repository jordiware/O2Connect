using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Options;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace O2Connect.Api.Crypto;

public interface ISigningKeyProvider
{
    IReadOnlyCollection<SigningKey> GetValidSigningKeys();
    bool TryGetActiveKey([NotNullWhen(true)] out SigningKey? activeKey);
}

public class RsaSigningKeyProvider : ISigningKeyProvider, IDisposable
{
    private readonly JwtOptions _options;

    private readonly object _lock = new();
    private readonly Dictionary<string, SigningKey> _keys = new();
    private readonly HashSet<RSA> _ownedRsa = new();

    public RsaSigningKeyProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;

        var rsa = RSA.Create();

        if (!File.Exists("private_key.pem"))
            throw new InvalidOperationException("Private key file missing.");

        rsa.ImportFromPem(File.ReadAllText("private_key.pem"));

        AddKey(rsa, _options.ActiveKeyId);
        SetActiveKey(_options.ActiveKeyId);
    }

    public IReadOnlyCollection<SigningKey> GetValidSigningKeys()
    {
        lock (_lock)
        {
            return _keys.Values.Where(k => k.Status != SigningKeyStatus.Expired).ToList();
        }
    }

    public bool TryGetActiveKey([NotNullWhen(true)] out SigningKey? activeKey)
    {
        lock (_lock)
        {
            activeKey = _keys.Values.SingleOrDefault(k => k.Status == SigningKeyStatus.Active);
            return activeKey is not null;
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
            _ownedRsa.Add(rsa);

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

            var updated = new Dictionary<string, SigningKey>();

            foreach (var (key, value) in _keys)
            {
                if (key == newActiveKeyId)
                    updated[key] = value.Clone(SigningKeyStatus.Active);
                else if (value.Status == SigningKeyStatus.Active)
                    updated[key] = value.Clone(SigningKeyStatus.Retired);
                else
                    updated[key] = value;
            }

            if (updated.Values.Count(k => k.Status == SigningKeyStatus.Active) != 1)
                throw new InvalidOperationException("Exactly one active key required.");

            _keys.Clear();
            foreach (var kv in updated)
                _keys.Add(kv.Key, kv.Value);
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var rsa in _ownedRsa)
            {
                rsa.Dispose();
            }
        }
    }
}
