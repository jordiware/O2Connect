using O2Connect.Api.Crypto;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using System.Security.Cryptography;

namespace O2Connect.Api.Services;

public interface ILoginService
{
    Task<User?> ValidateCredentialsAsync(string username, string password, CancellationToken ct);
}

public class LoginService : ILoginService
{
    private static readonly string DummyHash = CreateDummyHash();

    private readonly IUserRepository _userRepository;
    private readonly ISecretHasher _secretHasher;

    public LoginService(
        IUserRepository userRepository,
        ISecretHasher secretHasher)
    {
        _userRepository = userRepository;
        _secretHasher = secretHasher;
    }

    public async Task<User?> ValidateCredentialsAsync(string username, string password, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var storedUser = await _userRepository.GetByUsernameAsync(username.Trim(), ct);

        var hashToVerify = storedUser?.PasswordHash ?? DummyHash;

        var isValid = _secretHasher.Verify(password, hashToVerify);

        if (storedUser is null || !isValid)
            return null;

        if (_secretHasher.NeedsRehash(storedUser.PasswordHash))
        {
            if (_secretHasher.TryHash(password, out var newHash))
            {
                storedUser = storedUser with { PasswordHash = newHash };
                await _userRepository.UpdateAsync(storedUser, ct);
            }
        }

        return storedUser;
    }

    private static string CreateDummyHash()
    {
        var hasher = new Pbkdf2SecretHasher();

        var dummySecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        
        if (!hasher.TryHash(dummySecret, out var dummyHash))
            throw new InvalidOperationException("Failed to create dummy hash.");

        return dummyHash;
    }
}
