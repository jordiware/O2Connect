using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
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

        if (entry.State != ParState.Created)
            throw OAuthException.FromInvalidRequest();

        if (entry.ExpiresAt < DateTimeOffset.UtcNow)
        {
            var updatedEntry = entry with { State = ParState.Expired };
            await _parEntryRepository.StoreAsync(updatedEntry.RequestUri, updatedEntry, ct);
            throw OAuthException.FromInvalidRequest();
        }

        var session = await _parSessionRepository.GetAsync(entry.RequestUri, ct);

        if (session is null)
            throw OAuthException.FromInvalidRequest();

        var username = httpContext.User?.Identity?.Name;

        if (string.IsNullOrWhiteSpace(username))
            return new RedirectResponse
            {
                Action = "redirect",
                RedirectUrl = $"/account/login?request_uri={Uri.EscapeDataString(requestUri)}"
            };

        var user = await _userRepository.GetByUsernameAsync(username, ct);

        if (user is null)
            throw OAuthException.FromUnauthorizedClient();

        session = session with { State = ParAuthState.Authenticated };
        await _parSessionRepository.StoreAsync(session, ct);

        var consent = await _userConsentRepository.GetAsync(user.Id, session.ClientId, ct);

        if (consent is null)
        {
            return new RedirectResponse
            {
                Action = "redirect",
                RedirectUrl = $"/account/consent?session_id={session.SessionId}"
            };
        }

        var sessionScopes = ValueSet.FromDataString(session.Scope, ' ').Values;

        if (!sessionScopes.IsSubsetOf(consent.GrantedScopes))
            throw OAuthException.FromInvalidScope();

        session = session with { State = ParAuthState.Consented };
        await _parSessionRepository.StoreAsync(session, ct);

        session = session with { State = ParAuthState.CodeIssued };
        await _parSessionRepository.StoreAsync(session, ct);

        return new RedirectResponse
        {
            Action = "redirect",
            RedirectUrl = $"{session.RedirectUri}?code={Uri.EscapeDataString(session.SessionId)}"
        };
    }
}
