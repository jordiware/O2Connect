using O2Connect.Api.Crypto;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;

namespace O2Connect.Api.Services;

public interface ILoginService
{
    Task<User?> ValidateCredentialsAsync(string username, string password, CancellationToken ct);
}

public class LoginService : ILoginService
{
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
        var storedUser = await _userRepository.GetByUsernameAsync(username, ct);

        if (storedUser is null)
            return null;

        if (!_secretHasher.Verify(password, storedUser.PasswordHash))
            return null;

        return storedUser;
    }
}
