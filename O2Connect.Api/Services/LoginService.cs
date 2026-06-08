using O2Connect.Api.Crypto;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using System.Security.Cryptography;

namespace O2Connect.Api.Services;

public interface ILoginService
{
    Task<User?> ValidateCredentialsAsync(string username,
                                         string password,
                                         CancellationToken ct);
    Task LogoutAsync(string token, CancellationToken ct);
}

public class LoginService : ILoginService
{
    private static readonly string DummyHash = CreateDummyHash();

    private readonly IUserRepository _userRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IParAuthorizationSessionRepository _parAuthorizationSessionRepository;
    private readonly ISecretHasher _secretHasher;

    public LoginService(
        IUserRepository userRepository,
        IClientRepository clientRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IParAuthorizationSessionRepository parAuthorizationSessionRepository,
        ISecretHasher secretHasher)
    {
        _userRepository = userRepository;
        _clientRepository = clientRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _parAuthorizationSessionRepository = parAuthorizationSessionRepository;
        _secretHasher = secretHasher;
    }

    public async Task<User?> ValidateCredentialsAsync(string username,
                                                      string password,
                                                      CancellationToken ct)
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

    public async Task LogoutAsync(string token, CancellationToken ct)
    {
        await _refreshTokenRepository.RevokeAsync(token, ct);
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
