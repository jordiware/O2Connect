using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Crypto;
using O2Connect.Api.DataFactories;
using O2Connect.Api.DataFactories.RequestModels;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace O2Connect.Api.Services;

public interface IAccountService
{
    Task<HandleResult<LoginResult>> HandleLoginAsync(string? sessionId,
                                                     LoginRequest request,
                                                     CancellationToken ct);
    Task HandleLogoutAsync(EndSessionRequest request, CancellationToken ct);
    Task HandleLogoutAsync(string token, CancellationToken ct);
}

public class AccountService : IAccountService
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    private readonly IUserRepository _userRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IParAuthorizationSessionRepository _parAuthorizationSessionRepository;
    private readonly ITokenFactory _tokenFactory;
    private readonly ISecretHasher _secretHasher;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public AccountService(
        IUserRepository userRepository,
        IClientRepository clientRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IParAuthorizationSessionRepository parAuthorizationSessionRepository,
        ITokenFactory tokenFactory,
        ISecretHasher secretHasher,
        TokenValidationParameters tokenValidationParameters)
    {
        _userRepository = userRepository;
        _clientRepository = clientRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _parAuthorizationSessionRepository = parAuthorizationSessionRepository;
        _tokenFactory = tokenFactory;
        _secretHasher = secretHasher;
        _tokenValidationParameters = tokenValidationParameters;
    }

    public async Task<HandleResult<LoginResult>> HandleLoginAsync(string? sessionId,
                                                                  LoginRequest request,
                                                                  CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return HandleResult<LoginResult>.BadRequest("Invalid credentials");

        if (string.IsNullOrWhiteSpace(request.ClientId))
            return HandleResult<LoginResult>.BadRequest("Invalid client");

        var user = await ValidateCredentialsAsync(request.Username.Trim(), request.Password, ct);

        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct);

        if (user is null || client is null)
            return HandleResult<LoginResult>.Unauthorized("Invalid credentials");

        var allowedScopes = user.Scopes.Intersect(client.AllowedScopes);

        if (!allowedScopes.Any())
            return HandleResult<LoginResult>.Forbidden();

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var session = await _parAuthorizationSessionRepository.GetAsync(sessionId, ct);

            if (session is null || session.Stage != ParAuthStatus.AwaitingLogin)
                return HandleResult<LoginResult>.BadRequest("Invalid session");

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

            return HandleResult<LoginResult>.Success(new LoginRedirect(redirectResponse));
        }

        var additionalClaims = new Dictionary<string, object>
        {
            { "name", user.Username }
        };

        if (user.Roles is not null && user.Roles.Count > 0)
            additionalClaims["roles"] = user.Roles.ToArray();

        var tokenFactoryRequest = new JwtTokenFactoryRequest
        {
            ClientId = client.ClientId,
            Subject = user.Id,
            Scopes = allowedScopes.ToHashSet(),
            AdditionalClaims = additionalClaims
        };

        var tokenResponse = await _tokenFactory.GenerateAsync(tokenFactoryRequest, ct);

        return HandleResult<LoginResult>.Success(new LoginTokenSuccess(tokenResponse));
    }

    public async Task HandleLogoutAsync(EndSessionRequest request, CancellationToken ct)
    {
        await HandleLogoutAsync(request.IdTokenHint, ct);
    }

    public async Task HandleLogoutAsync(string? token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        ClaimsPrincipal principal;

        try
        {
            principal = TokenHandler.ValidateToken(token,
                                                   _tokenValidationParameters,
                                                   out var validatedToken);

            if (validatedToken is not JwtSecurityToken)
                return;
        }
        catch (SecurityTokenException)
        {
            return;
        }

        var sessionId = principal.FindFirst("sid")?.Value;

        if (!string.IsNullOrWhiteSpace(sessionId))
            await _refreshTokenRepository.RevokeSessionAsync(sessionId, ct);
    }

    private async Task<User?> ValidateCredentialsAsync(string username,
                                                      string password,
                                                      CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var storedUser = await _userRepository.GetByUsernameAsync(username.Trim(), ct);

        var hashToVerify = storedUser?.PasswordHash ?? CreateDummyHash();

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
