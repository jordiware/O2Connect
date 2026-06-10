using O2Connect.Api.DataFactories;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.Collections.Immutable;

namespace O2Connect.Api.Services;

public interface IConsentService
{
    Task<bool> SetConsentGrantedSessionAsync(string sessionId,
                                             CancellationToken ct);
    Task DeleteSessionAsync(string sessionId,
                            CancellationToken ct);
    Task<ConsentEvaluationResult> EvaluateAsync(string userId,
                                                string clientId,
                                                IReadOnlySet<string> requestedScopes,
                                                CancellationToken ct);
    Task<AuthorizationSession?> GetSessionAsync(string sessionId,
                                                CancellationToken ct);
    Task SaveConsentAsync(string userId,
                          string clientId,
                          IReadOnlySet<string> approvedScopes,
                          CancellationToken ct);
    Task<RedirectResponse> HandleParConsentAsync(string sessionId,
                                                 ConsentDecisionRequest request,
                                                 CancellationToken ct);
}

public class ConsentService : IConsentService
{
    private readonly IUserConsentRepository _userConsentRepository;
    private readonly IAuthorizationSessionRepository _authorizationSessionRepository;
    private readonly IParEntryRepository _parEntryRepository;

    public ConsentService(
        IUserConsentRepository userConsentRepository,
        IAuthorizationSessionRepository authorizationSessionRepository,
        IParEntryRepository parEntryRepository)
    {
        _userConsentRepository = userConsentRepository;
        _authorizationSessionRepository = authorizationSessionRepository;
        _parEntryRepository = parEntryRepository;
    }

    public async Task<ConsentEvaluationResult> EvaluateAsync(string userId,
                                                             string clientId,
                                                             IReadOnlySet<string> requestedScopes,
                                                             CancellationToken ct)
    {
        var existing = await _userConsentRepository.GetAsync(userId, clientId, ct);

        if (existing == null)
        {
            return new ConsentEvaluationResult
            {
                RequiresConsent = true,
                MissingScopes = requestedScopes
            };
        }

        var missing = requestedScopes.Except(existing.GrantedScopes).ToImmutableHashSet();

        return new ConsentEvaluationResult
        {
            RequiresConsent = missing.Any(),
            MissingScopes = missing
        };
    }

    public async Task<bool> SetConsentGrantedSessionAsync(string sessionId, CancellationToken ct)
    {
        var session = await _authorizationSessionRepository.GetAsync(sessionId, ct);

        if (session == null)
            return false;

        var updatedSession = session with { Status = AuthorizationStatus.Consented };

        await _authorizationSessionRepository.StoreAsync(updatedSession, ct);

        return true;
    }

    public async Task DeleteSessionAsync(string sessionId, CancellationToken ct)
    {
        await _authorizationSessionRepository.TryConsumeAsync(sessionId, ct);
    }

    public async Task<AuthorizationSession?> GetSessionAsync(string sessionId, CancellationToken ct)
    {
        return await _authorizationSessionRepository.GetAsync(sessionId, ct);
    }

    public async Task SaveConsentAsync(string userId,
                                       string clientId,
                                       IReadOnlySet<string> approvedScopes,
                                       CancellationToken ct)
    {
        var existing = await _userConsentRepository.GetAsync(userId, clientId, ct);

        if (existing == null)
        {
            var consent = new UserConsent
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                ClientId = clientId,
                GrantedScopes = approvedScopes,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _userConsentRepository.StoreAsync(consent, ct);
            return;
        }

        var grantedScopes = existing.GrantedScopes.Union(approvedScopes).ToHashSet();

        var updatedConsent = new UserConsent
        {
            Id = existing.Id,
            UserId = userId,
            ClientId = clientId,
            GrantedScopes = grantedScopes,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _userConsentRepository.StoreAsync(updatedConsent, ct);
    }

    public async Task<RedirectResponse> HandleParConsentAsync(string sessionId,
                                                              ConsentDecisionRequest request,
                                                              CancellationToken ct)
    {
        var session = await _authorizationSessionRepository.GetAsync(sessionId, ct);

        if (session is null)
            throw OAuthException.FromInvalidRequest("Invalid session");

        if (session.Status != AuthorizationStatus.ConsentRequired)
            throw OAuthException.FromInvalidRequest("Invalid session state for consent");

        if (session.UserId is null)
            throw OAuthException.FromAccessDenied("User not authenticated");

        var entry = await _parEntryRepository.GetAsync(session.RequestUriCode!, ct);

        if (entry is null)
            throw OAuthException.FromInvalidRequest("PAR entry missing");

        var requestedScopes = ParseScope(entry.Scope);

        if (!request.Approved)
        {
            session = session with { Status = AuthorizationStatus.Aborted };
            await _authorizationSessionRepository.StoreAsync(session, ct);
            return BuildErrorRedirect(entry, "access_denied");
        }

        await SaveConsentAsync(session.UserId,
                               entry.ClientId,
                               requestedScopes.ToImmutableHashSet(),
                               ct);

        session = session with { Status = AuthorizationStatus.Consented };
        await _authorizationSessionRepository.StoreAsync(session, ct);

        var requestUri = BuildRequestUri(session.RequestUriCode!);
        return new RedirectResponse
        {
            Action = "redirect",
            RedirectUrl = RedirectUrlFactory.Authorize(requestUri)
        };
    }

    private static HashSet<string> ParseScope(string scope)
    {
        return scope.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .ToHashSet(StringComparer.Ordinal);
    }

    private static string BuildRequestUri(string code)
    {
        return $"urn:ietf:params:oauth:request_uri:{code}";
    }

    private static RedirectResponse BuildErrorRedirect(ParEntry entry, string error)
    {
        var uri = new UriBuilder(entry.RedirectUri);

        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        query["error"] = error;

        if (!string.IsNullOrEmpty(entry.State))
            query["state"] = entry.State;

        uri.Query = query.ToString()!;

        return new RedirectResponse
        {
            Action = "redirect",
            RedirectUrl = uri.ToString()
        };
    }
}
