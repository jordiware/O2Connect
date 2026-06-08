using O2Connect.Api.Crypto;
using O2Connect.Api.DataFactories;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Responses;
using System.Collections.Immutable;
using System.Security.Cryptography;

namespace O2Connect.Api.Services;

public interface ILoginService
{
    Task<RedirectResponse> HandleWithSessionAsync(string username,
                                                  string password,
                                                  string clientId,
                                                  string sessionId,
                                                  CancellationToken ct);
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

    public async Task<RedirectResponse> HandleWithSessionAsync(string username,
                                                               string password,
                                                               string clientId,
                                                               string sessionId,
                                                               CancellationToken ct)
    {
        var user = await ValidateCredentialsAsync(username, password, ct);
        var session = await _parAuthorizationSessionRepository.GetAsync(sessionId, ct);
        var client = await _clientRepository.GetByIdAsync(clientId, ct);

        if (user is null)
            throw OAuthException.FromAccessDenied();

        if (session is null || session.Stage != ParAuthStatus.AwaitingLogin)
            throw OAuthException.FromInvalidRequest();

        if (client is null)
            throw OAuthException.FromInvalidRequest();

        var allowedScopes = user.Scopes.Intersect(client.AllowedScopes);
        if (!allowedScopes.Any())
            throw OAuthException.FromAccessDenied();

        session = session with
        {
            Stage = ParAuthStatus.Authenticated,
            UserId = user.Id
        };

        await _parAuthorizationSessionRepository.StoreAsync(session, ct);

        return new RedirectResponse
        {
            Action = "redirect",
            RedirectUrl = RedirectUrlFactory.Authorize($"urn:ietf:params:oauth:request_uri:{session.RequestUriCode}")
        };
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
