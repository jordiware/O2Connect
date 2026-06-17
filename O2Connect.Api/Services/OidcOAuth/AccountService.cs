using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Crypto;
using O2Connect.Api.DataFactories;
using O2Connect.Api.DataFactories.RequestModels;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace O2Connect.Api.Services.OidcOAuth;

public interface IAccountService
{
    Task<LoginResult> HandleLoginAsync(string? sessionId, LoginRequest request, CancellationToken ct);
    Task HandleLogoutAsync(EndSessionRequest request, CancellationToken ct);
    Task HandleLogoutAsync(string token, CancellationToken ct);
    Task<RegisterUserResponse> PatchMeAsync(string userId, UpdateUserRequest request, CancellationToken ct);
    Task<RegisterUserResponse> HandleRegisterAsync(RegisterUserRequest request, CancellationToken ct);
    Task ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken ct);
}

public class AccountService : IAccountService
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();
    private static readonly Regex UsernameVerifierRegex = new("^(?=.{3,32}$)(?!.*[._-]{2})[a-zA-Z0-9]+([._-]?[a-zA-Z0-9]+)*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly HashSet<string> ReservedUsernames =
    [
        "admin",
        "root",
        "system",
        "me",
        "null"
    ];

    private readonly IUserRepository _userRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuthorizationSessionRepository _authorizationSessionRepository;
    private readonly ITokenFactory _tokenFactory;
    private readonly ISecretHasher _secretHasher;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public AccountService(
        IUserRepository userRepository,
        IClientRepository clientRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAuthorizationSessionRepository authorizationSessionRepository,
        ITokenFactory tokenFactory,
        ISecretHasher secretHasher,
        TokenValidationParameters tokenValidationParameters)
    {
        _userRepository = userRepository;
        _clientRepository = clientRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _authorizationSessionRepository = authorizationSessionRepository;
        _tokenFactory = tokenFactory;
        _secretHasher = secretHasher;
        _tokenValidationParameters = tokenValidationParameters;
    }

    public async Task<LoginResult> HandleLoginAsync(string? sessionId, LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            throw OAuthException.FromInvalidRequest("Invalid credentials");

        if (string.IsNullOrWhiteSpace(request.ClientId))
            throw OAuthException.FromInvalidRequest("Invalid client");

        var user = await ValidateCredentialsAsync(request.Username.Trim(), request.Password, ct);

        var client = await _clientRepository.GetAsync(request.ClientId, ct);

        if (user is null || client is null)
            throw OAuthException.FromInvalidGrant("Invalid credentials");

        var allowedScopes = user.Scopes.Intersect(client.AllowedScopes);

        if (!allowedScopes.Any())
            throw OAuthException.FromAccessDenied("User is not allowed to access this client");

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var session = await _authorizationSessionRepository.GetAsync(sessionId, ct);

            if (session is null || session.Status != AuthorizationStatus.LoginRequired)
                throw OAuthException.FromInvalidRequest("Invalid session");

            session = session with
            {
                Status = AuthorizationStatus.Authenticated,
                UserId = user.Id
            };

            await _authorizationSessionRepository.StoreAsync(session, ct);

            var redirectResponse = new RedirectResponse
            {
                Action = "redirect",
                RedirectUrl = RedirectUrlFactory.Authorize($"urn:ietf:params:oauth:request_uri:{session.RequestUriCode}")
            };

            return new LoginRedirect(redirectResponse);
        }

        var additionalClaims = new Dictionary<string, object>
        {
            ["name"] = user.Username,
            ["role"] = user.Role
        };

        var tokenFactoryRequest = new JwtTokenFactoryRequest
        {
            ClientId = client.Id,
            Subject = user.Id,
            Scopes = allowedScopes.ToHashSet(),
            AdditionalClaims = additionalClaims
        };

        var tokenResponse = await _tokenFactory.GenerateAsync(tokenFactoryRequest, ct);

        return new LoginTokenSuccess(tokenResponse);
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

    public async Task<RegisterUserResponse> PatchMeAsync(string userId,
                                                         UpdateUserRequest request,
                                                         CancellationToken ct)
    {
        var user = await _userRepository.GetAsync(userId, ct);

        if (user is null)
            throw OAuthException.FromInvalidGrant();

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            var displayName = request.DisplayName.Trim();

            ValidateDisplayName(displayName);

            user = user with { DisplayName = request.DisplayName };
        }

        if (!string.IsNullOrWhiteSpace(request.PictureUri))
        {
            var pictureUri = request.PictureUri.Trim();

            ValidateUri(pictureUri);

            user = user with { PictureUri = request.PictureUri };
        }

        await _userRepository.StoreAsync(user, ct);

        return new RegisterUserResponse
        {
            UserId = user.Id
        };
    }

    public async Task<RegisterUserResponse> HandleRegisterAsync(RegisterUserRequest request,
                                                                CancellationToken ct)
    {
        ValidateUsername(request.Username);
        ValidatePassword(request.Password);

        var normalizedUsername = NormalizeUsername(request.Username);

        if (ReservedUsernames.Contains(normalizedUsername))
            throw OAuthException.FromInvalidRequest();

        if (await _userRepository.ContainsUserAsync(normalizedUsername, ct))
            throw OAuthException.FromInvalidRequest();

        var scopes = ValueSet.FromDataString(request.Scopes, ' ').Values;

        if (!_secretHasher.TryHash(request.Password, out var passwordHash))
            throw OAuthException.FromServerError();

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username,
            NormalizedUsername = normalizedUsername,
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = request.Role ?? UserRole.User,
            Scopes = scopes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            DisplayName = request.DisplayName,
            PictureUri = request.PictureUri
        };

        await _userRepository.StoreAsync(user, ct);

        return new RegisterUserResponse
        {
            UserId = user.Id
        };
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken ct)
    {
        ValidatePassword(request.NewPassword);

        var user = await _userRepository.GetAsync(userId, ct);

        if (user is null)
            throw OAuthException.FromInvalidGrant();

        if (!_secretHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw OAuthException.FromInvalidGrant();

        if (!_secretHasher.TryHash(request.NewPassword, out var passwordHash))
            throw OAuthException.FromServerError();

        user = user with { PasswordHash = passwordHash };

        await _userRepository.StoreAsync(user, ct);

        await _refreshTokenRepository.RevokeSubjectAsync(userId, ct);
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
                await _userRepository.StoreAsync(storedUser, ct);
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

    private static void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw OAuthException.FromInvalidRequest();

        if (username.Length < 3 || username.Length > 32)
            throw OAuthException.FromInvalidRequest();

        if (!UsernameVerifierRegex.IsMatch(username))
            throw OAuthException.FromInvalidRequest();
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw OAuthException.FromInvalidRequest();

        if (password.Length < 8 || password.Length > 128)
            throw OAuthException.FromInvalidRequest();

        bool hasLower = false;
        bool hasUpper = false;
        bool hasDigit = false;

        foreach (var c in password)
        {
            if (char.IsLower(c)) hasLower = true;
            else if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsDigit(c)) hasDigit = true;
        }

        if (!hasLower || !hasUpper || !hasDigit)
            throw OAuthException.FromInvalidRequest();
    }

    private static void ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw OAuthException.FromInvalidRequest();
        if (displayName.Length > 32)
            throw OAuthException.FromInvalidRequest();
        
        foreach (var c in displayName)
        {
            if (char.IsControl(c))
                throw OAuthException.FromInvalidRequest();
        }
    }

    private static void ValidateUri(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw OAuthException.FromInvalidRequest();

        if (value.Length > 2048)
            throw OAuthException.FromInvalidRequest("picture_uri too long.");

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            throw OAuthException.FromInvalidRequest("picture_uri must be a valid absolute URI.");

        if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            throw OAuthException.FromInvalidRequest("picture_uri must use http or https.");
    }

    private static string NormalizeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw OAuthException.FromInvalidRequest();

        var trimmed = username.Trim();

        var normalized = trimmed.Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);

            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString()
                 .Normalize(NormalizationForm.FormKC)
                 .ToLowerInvariant();
    }
}
