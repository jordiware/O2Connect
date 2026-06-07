using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.Collections.Immutable;
using System.Net.NetworkInformation;

namespace O2Connect.Api.Services;

public interface IConsentService
{
    Task<bool> SetConsentGrantedSessionAsync(string sessionId,
                                             CancellationToken ct);
    Task DeleteSessionAsync(string sessionId,
                            CancellationToken ct);
    Task<ConsentEvaluationResult> EvaluateAsync(string userId,
                                                string clientId,
                                                ImmutableHashSet<string> requestedScopes,
                                                CancellationToken ct);
    Task<AuthorizationSession?> GetSessionAsync(string sessionId,
                                                CancellationToken ct);
    Task SaveConsentAsync(string userId,
                          string clientId,
                          ImmutableHashSet<string> approvedScopes,
                          CancellationToken ct);
    Task<RedirectResponse> HandleParSessionAsync(string sessionId,
                                                 ConsentDecisionRequest request,
                                                 CancellationToken ct);
}

public class ConsentService : IConsentService
{
    private readonly IUserConsentRepository _userConsentRepository;
    private readonly IAuthorizationSessionRepository _authorizationSessionRepository;
    private readonly IParAuthorizationSessionRepository _parAuthorizationSessionRepository;
    private readonly IParEntryRepository _parEntryRepository;

    public ConsentService(
        IUserConsentRepository userConsentRepository,
        IAuthorizationSessionRepository authorizationSessionRepository,
        IParAuthorizationSessionRepository parAuthorizationSessionRepository,
        IParEntryRepository parEntryRepository)
    {
        _userConsentRepository = userConsentRepository;
        _authorizationSessionRepository = authorizationSessionRepository;
        _parAuthorizationSessionRepository = parAuthorizationSessionRepository;
        _parEntryRepository = parEntryRepository;
    }

    public async Task<ConsentEvaluationResult> EvaluateAsync(string userId,
                                                             string clientId,
                                                             ImmutableHashSet<string> requestedScopes,
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

        var updatedSession = session with { Stage = AuthorizationStage.ConsentGranted };

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
                                       ImmutableHashSet<string> approvedScopes,
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

        var grantedScopes = existing.GrantedScopes.Union(approvedScopes);

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

    public async Task<RedirectResponse> HandleParSessionAsync(string sessionId,
                                                              ConsentDecisionRequest request,
                                                              CancellationToken ct)
    {
        var session = await _parAuthorizationSessionRepository.GetAsync(sessionId, ct);

        if (session is null)
            throw OAuthException.FromInvalidRequest("Invalid session");

        if (session.Status != ParAuthStatus.AwaitingConsent)
            throw OAuthException.FromInvalidRequest("Invalid session state for consent");

        if (session.UserId is null)
            throw OAuthException.FromAccessDenied("User not authenticated");

        var entry = await _parEntryRepository.GetAsync(session.RequestUriCode, ct);

        if (entry is null)
            throw OAuthException.FromInvalidRequest("PAR entry missing");

        var requestedScopes = ParseScope(entry.Scope);

        if (!request.Approved)
        {
            session = session with { Status = ParAuthStatus.Aborted };
            await _parAuthorizationSessionRepository.StoreAsync(session, ct);
            return BuildErrorRedirect(entry, "access_denied");
        }

        await SaveConsentAsync(session.UserId,
                               entry.ClientId,
                               requestedScopes.ToImmutableHashSet(),
                               ct);

        session = session with { Status = ParAuthStatus.Consented };
        await _parAuthorizationSessionRepository.StoreAsync(session, ct);

        return new RedirectResponse
        {
            Action = "redirect",
            RedirectUrl = $"/connect/authorize?request_uri={BuildRequestUri(session.RequestUriCode)}"
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
