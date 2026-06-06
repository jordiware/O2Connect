using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Responses;
using System.Security.Cryptography;

namespace O2Connect.Api.Services;

public interface IParAuthorizationService
{
    Task<RedirectResponse> HandleAsync(string requestUri,
                                       HttpContext httpContext,
                                       CancellationToken ct);
}

public class ParAuthorizationService : IParAuthorizationService
{
    private readonly IParEntryRepository _parEntryRepository;
    private readonly IParAuthorizationSessionRepository _parSessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserConsentRepository _userConsentRepository;

    public ParAuthorizationService(
        IParEntryRepository parEntryRepository,
        IParAuthorizationSessionRepository parAuthorizationSessionRepository,
        IUserRepository userRepository,
        IUserConsentRepository userConsentRepository)
    {
        _parEntryRepository = parEntryRepository;
        _parSessionRepository = parAuthorizationSessionRepository;
        _userRepository = userRepository;
        _userConsentRepository = userConsentRepository;
    }

    public async Task<RedirectResponse> HandleAsync(string requestUri,
                                                    HttpContext httpContext,
                                                    CancellationToken ct)
    {
        var utcNow = DateTimeOffset.UtcNow;

        var entry = await _parEntryRepository.GetAsync(requestUri, ct);

        if (entry is null)
            throw OAuthException.FromInvalidRequest();

        if (entry.ExpiresAt <= utcNow)
        {
            entry = entry with { Status = ParStatus.Expired };

            await _parEntryRepository.StoreAsync(entry.RequestUri, entry, ct);

            throw OAuthException.FromInvalidRequest();
        }

        if (entry.Status != ParStatus.Created)
            throw OAuthException.FromInvalidRequest();

        if (!string.Equals(entry.ResponseType, "code", StringComparison.Ordinal))
            throw OAuthException.FromUnsupportedResponseType();

        var session = await _parSessionRepository.GetFromRequestUriAsync(requestUri, ct);

        if (session is null)
            throw OAuthException.FromInvalidRequest();

        if (session.ExpiresAt <= utcNow)
        {
            session = session with { Status = ParAuthStatus.Aborted };

            await _parSessionRepository.StoreAsync(session, ct);

            throw OAuthException.FromInvalidRequest();
        }

        if (!string.Equals(entry.RedirectUri, session.RedirectUri, StringComparison.Ordinal))
            throw OAuthException.FromInvalidRequest();

        if (!string.Equals(entry.ClientId, session.ClientId, StringComparison.Ordinal))
            throw OAuthException.FromInvalidRequest();

        if (!string.Equals(entry.Scope, session.Scope, StringComparison.Ordinal))
            throw OAuthException.FromInvalidRequest();

        var username = httpContext.User?.Identity?.Name;

        if (string.IsNullOrWhiteSpace(username))
        {
            session = session with { Status = ParAuthStatus.AwaitingLogin };
            await _parSessionRepository.StoreAsync(session, ct);

            return new RedirectResponse
            {
                Action = "redirect",
                RedirectUrl = $"/account/login?session_id={Uri.EscapeDataString(session.SessionId)}"
            };
        }

        var user = await _userRepository.GetByUsernameAsync(username, ct);

        if (user is null)
            throw OAuthException.FromUnauthorizedClient();

        session = session with
        {
            Status = ParAuthStatus.Authenticated,
            UserId = user.Id
        };
        await _parSessionRepository.StoreAsync(session, ct);

        var consent = await _userConsentRepository.GetAsync(user.Id, session.ClientId, ct);

        if (consent is null)
        {
            session = session with { Status = ParAuthStatus.AwaitingConsent };
            await _parSessionRepository.StoreAsync(session, ct);

            return new RedirectResponse
            {
                Action = "redirect",
                RedirectUrl = $"/account/consent?session_id={session.SessionId}"
            };
        }

        var sessionScopes = ValueSet.FromDataString(session.Scope, ' ').Values;

        if (!sessionScopes.IsSubsetOf(consent.GrantedScopes))
        {
            session = session with { Status = ParAuthStatus.AwaitingConsent };
            await _parSessionRepository.StoreAsync(session, ct);

            return new RedirectResponse
            {
                Action = "redirect",
                RedirectUrl = $"/account/consent?session_id={session.SessionId}"
            };
        }

        session = session with { Status = ParAuthStatus.Consented };
        await _parSessionRepository.StoreAsync(session, ct);

        var code = GenerateCode();
        session = session with
        {
            Status = ParAuthStatus.CodeIssued,
            Code = code
        };
        await _parSessionRepository.StoreAsync(session, ct);

        entry = entry with { Status = ParStatus.Consumed };
        await _parEntryRepository.StoreAsync(entry.RequestUri, entry, ct);

        var query = new List<string>
        {
            $"code={Uri.EscapeDataString(code)}"
        };

        if (!string.IsNullOrEmpty(session.State))
        {
            query.Add($"state={Uri.EscapeDataString(session.State)}");
        }

        return new RedirectResponse
        {
            Action = "redirect",
            RedirectUrl = $"{session.RedirectUri}?{string.Join('&', query)}"
        };
    }

    private static string GenerateCode()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);

        var code = Convert.ToBase64String(bytes)
                          .TrimEnd('=')
                          .Replace('+', '-')
                          .Replace('/', '_');

        return code;
    }

}
