using O2Connect.Api.Crypto;
using O2Connect.Api.DataFactories;
using O2Connect.Api.DataFactories.RequestModels;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.Security.Cryptography;

namespace O2Connect.Api.Services;

public interface ILoginService
{
    Task<LoginResult> HandleAsync(string? sessionId, LoginRequest request, CancellationToken ct);
    Task LogoutAsync(string token, CancellationToken ct);
}

public class LoginService : ILoginService
{
    private static readonly string DummyHash = CreateDummyHash();

    private readonly IUserRepository _userRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IParAuthorizationSessionRepository _parAuthorizationSessionRepository;
    private readonly ITokenFactory _tokenFactory;
    private readonly ISecretHasher _secretHasher;

    public LoginService(
        IUserRepository userRepository,
        IClientRepository clientRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IParAuthorizationSessionRepository parAuthorizationSessionRepository,
        ITokenFactory tokenFactory,
        ISecretHasher secretHasher)
    {
        _userRepository = userRepository;
        _clientRepository = clientRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _parAuthorizationSessionRepository = parAuthorizationSessionRepository;
        _tokenFactory = tokenFactory;
        _secretHasher = secretHasher;
    }

    public async Task<LoginResult> HandleAsync(string? sessionId,
                                               LoginRequest request,
                                               CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return new LoginBadRequest("Invalid credentials");

        if (string.IsNullOrWhiteSpace(request.ClientId))
            return new LoginBadRequest("Invalid client");

        var user = await ValidateCredentialsAsync(request.Username.Trim(), request.Password, ct);

        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct);

        if (user is null || client is null)
            return new LoginUnauthorized("Invalid credentials");

        var allowedScopes = user.Scopes.Intersect(client.AllowedScopes);

        if (!allowedScopes.Any())
            return new LoginForbidden();

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var session = await _parAuthorizationSessionRepository.GetAsync(sessionId, ct);

            if (session is null || session.Stage != ParAuthStatus.AwaitingLogin)
                return new LoginBadRequest("Invalid session");

            session = session with
            {
                Stage = ParAuthStatus.Authenticated,
                UserId = user.Id
            };

            await _parAuthorizationSessionRepository.StoreAsync(session, ct);

            var redirectResponse = new RedirectResponse
            {
                Action = "redirect",
                RedirectUrl = RedirectUrlFactory.Authorize($"urn:ietf:params:oauth:request_uri:{session.RequestUriCode}")
            };

            return new LoginRedirect(redirectResponse);
        }

        var tokenFactoryRequest = new JwtTokenFactoryRequest
        {
            ClientId = client.ClientId,
            Subject = user.Id,
            Scopes = allowedScopes.ToHashSet(),
            AdditionalClaims = new Dictionary<string, object>
            {
                { "name", user.Username }
            }
        };

        if (user.Roles is not null && user.Roles.Count > 0)
        {
            tokenFactoryRequest.AdditionalClaims["roles"] = user.Roles.ToArray();
        }

        var tokenResponse = await _tokenFactory.GenerateAsync(tokenFactoryRequest, ct);

        return new LoginTokenSuccess(tokenResponse);
    }

    public async Task LogoutAsync(string token, CancellationToken ct)
    {
        await _refreshTokenRepository.RevokeAsync(token, ct);
    }

    private async Task<User?> ValidateCredentialsAsync(string username,
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

    private static string CreateDummyHash()
    {
        var hasher = new Pbkdf2SecretHasher();

        var dummySecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

        if (!hasher.TryHash(dummySecret, out var dummyHash))
            throw new InvalidOperationException("Failed to create dummy hash.");

        return dummyHash;
    }
}
