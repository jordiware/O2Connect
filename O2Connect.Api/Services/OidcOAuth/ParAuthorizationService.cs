using O2Connect.Api.Crypto;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.OidcOAuth;

namespace O2Connect.Api.Services.OidcOAuth;

public interface IParAuthorizationService
{
    Task<RedirectResponse> HandleAsync(string requestUri,
                                       HttpContext httpContext,
                                       CancellationToken ct);
}

public class ParAuthorizationService : IParAuthorizationService
{
    private readonly IParEntryRepository _parEntryRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IAuthorizationSessionRepository _authorizationSessionRepository;
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IUserConsentRepository _userConsentRepository;

    public ParAuthorizationService(
        IParEntryRepository parEntryRepository,
        IClientRepository clientRepository,
        IAuthorizationSessionRepository authorizationSessionRepository,
        IAuthorizationCodeRepository authorizationCodeRepository,
        IUserConsentRepository userConsentRepository)
    {
        _parEntryRepository = parEntryRepository;
        _clientRepository = clientRepository;
        _authorizationSessionRepository = authorizationSessionRepository;
        _authorizationCodeRepository = authorizationCodeRepository;
        _userConsentRepository = userConsentRepository;
    }

    public async Task<RedirectResponse> HandleAsync(string requestUri,
                                                    HttpContext httpContext,
                                                    CancellationToken ct)
    {
        var code = ExtractCode(requestUri);

        var entry = await _parEntryRepository.GetAsync(code, ct);

        if (entry is null)
            throw OAuthException.FromInvalidRequest();

        var session = await _authorizationSessionRepository.GetFromRequestUriCodeAsync(requestUri, ct);

        if (session is null)
            throw OAuthException.FromInvalidRequest();

        var utcNow = DateTimeOffset.UtcNow;

        if (entry.ExpiresAt <= utcNow || session.ExpiresAt <= utcNow)
            throw OAuthException.FromInvalidRequest();

        var client = await _clientRepository.GetAsync(entry.ClientId, ct);

        if (client is null) 
            throw OAuthException.FromInvalidClient();

        if (session.Status is AuthorizationStatus.CodeIssued or AuthorizationStatus.Aborted)
            throw OAuthException.FromInvalidRequest();

        var user = httpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            await UpdateSessionAsync(session with { Status = AuthorizationStatus.LoginRequired }, ct);

            return BuildRedirect("/login?session=" + session.SessionId);
        }

        var userId = user.FindFirst("sub")?.Value;

        if (userId is null)
            throw OAuthException.FromAccessDenied();

        session = session with
        {
            Status = AuthorizationStatus.Authenticated,
            UserId = userId
        };

        await UpdateSessionAsync(session, ct);

        var requestedScopes = ParseScope(entry.Scope);

        var missingScopes = await GetMissingScopes(userId, client.Id, requestedScopes, ct);

        if (missingScopes.Count > 0)
        {
            await UpdateSessionAsync(session with { Status = AuthorizationStatus.ConsentRequired }, ct);

            return BuildRedirect("/consent?session=" + session.SessionId);
        }

        return await IssueCode(entry, session, ct);
    }

    private async Task UpdateSessionAsync(AuthorizationSession session, CancellationToken ct)
    {
        await _authorizationSessionRepository.StoreAsync(session, ct);
    }

    private async Task<RedirectResponse> IssueCode(
        ParEntry entry,
        AuthorizationSession session,
        CancellationToken ct)
    {
        var authCode = SecureCodeGenerator.GenerateBase64UrlToken(length: 32);

        await _authorizationCodeRepository.StoreAsync(new AuthorizationCode
        {
            Code = authCode,
            ClientId = entry.ClientId,
            RedirectUri = entry.RedirectUri,
            Scopes = entry.Scope,
            CodeChallenge = entry.CodeChallenge,
            CodeChallengeMethod = entry.CodeChallengeMethod,
            UserId = session.UserId!,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
        }, ct);

        await UpdateSessionAsync(session with { Status = AuthorizationStatus.CodeIssued }, ct);

        var redirectUrl = BuildCallbackUrl(entry, authCode);

        return new RedirectResponse
        {
            Action = "redirect",
            RedirectUrl = redirectUrl
        };
    }

    private static string ExtractCode(string requestUri)
    {
        const string prefix = "urn:ietf:params:oauth:request_uri:";

        if (!requestUri.StartsWith(prefix, StringComparison.Ordinal))
            throw OAuthException.FromInvalidRequest("Invalid request_uri format");

        return requestUri.Substring(prefix.Length);
    }

    private static RedirectResponse BuildRedirect(string url)
    {
        return new RedirectResponse
        {
            Action = "redirect",
            RedirectUrl = url
        };
    }

    private static string BuildCallbackUrl(ParEntry entry, string code)
    {
        var uri = new UriBuilder(entry.RedirectUri);

        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        query["code"] = code;

        if (!string.IsNullOrEmpty(entry.State))
            query["state"] = entry.State;

        uri.Query = query.ToString();

        return uri.ToString();
    }

    private static HashSet<string> ParseScope(string scope)
    {
        return scope.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .ToHashSet(StringComparer.Ordinal);
    }

    private async Task<HashSet<string>> GetMissingScopes(string userId,
                                                         string clientId,
                                                         HashSet<string> requestedScopes,
                                                         CancellationToken ct)
    {
        var storedConsent = await _userConsentRepository.GetAsync(userId, clientId, ct);

        var grantedScopes = storedConsent?.GrantedScopes.ToHashSet() ?? new HashSet<string>();

        return requestedScopes.Except(grantedScopes, StringComparer.Ordinal).ToHashSet();
    }
}
