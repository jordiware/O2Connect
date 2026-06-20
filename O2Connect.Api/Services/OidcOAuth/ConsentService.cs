using Microsoft.AspNetCore.WebUtilities;
using O2Connect.Api.DataFactories;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.OidcOAuth;
using O2Connect.Dto.OidcOAuth.Consent;
using System.Collections.Immutable;

namespace O2Connect.Api.Services.OidcOAuth;

public interface IConsentService
{
    Task<ConsentResponse> GetConsentResponse(string sessionId, CancellationToken ct);

    Task<RedirectResponse> HandleConsentAsync(string sessionId,
                                              ConsentDecisionRequest request,
                                              CancellationToken ct);
    Task<RedirectResponse> HandleParConsentAsync(string sessionId,
                                                 ConsentDecisionRequest request,
                                                 CancellationToken ct);
    Task<bool> IsParConsentSession(string sessionId, CancellationToken ct);

    Task<ConsentEvaluationResult> EvaluateAsync(string userId,
                                                string clientId,
                                                IReadOnlySet<string> requestedScopes,
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

    public async Task<ConsentResponse> GetConsentResponse(string sessionId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw OAuthException.FromInvalidRequest();

        var session = await GetSessionAsync(sessionId, ct);

        if (session == null || session.ExpiresAt <= DateTimeOffset.UtcNow)
            throw OAuthException.FromInvalidRequest("Consent session expired");

        if (session.MissingScopes == null || session.MissingScopes.Length == 0)
            throw OAuthException.FromInvalidRequest("No consent required for this session");

        var response = new ConsentResponse
        {
            SessionId = sessionId,
            ClientId = session.Request.ClientId,
            ClientName = session.ClientDisplayName,
            UserDisplayName = session.UserDisplayName,
            Scope = string.Join(' ', session.MissingScopes.Order())
        };

        return response;
    }

    public async Task<RedirectResponse> HandleConsentAsync(string sessionId,
                                                           ConsentDecisionRequest request,
                                                           CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw OAuthException.FromInvalidRequest();

        var session = await GetSessionAsync(sessionId, ct);

        if (session is null)
            throw OAuthException.FromInvalidRequest("Invalid session");

        if (session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            session = session with { Status = AuthorizationStatus.Expired };

            await _authorizationSessionRepository.StoreAsync(session, ct);

            throw OAuthException.FromInvalidRequest("Expired session");
        }

        if (session.Status != AuthorizationStatus.ConsentRequired)
            throw OAuthException.FromInvalidRequest("Invalid session state for consent");

        if (session.MissingScopes == null || session.MissingScopes.Length == 0)
            throw OAuthException.FromInvalidRequest("No consent required for this session");

        if (!request.Approved)
        {
            session = session with { Status = AuthorizationStatus.Cancelled };
            await _authorizationSessionRepository.StoreAsync(session, ct);

            return new RedirectResponse
            {
                Action = "deny",
                RedirectUrl = BuildErrorRedirect(session)
            };
        }

        if (string.IsNullOrWhiteSpace(request.ApprovedScopes)
            || ParseScope(request.ApprovedScopes).Count == 0)
            throw OAuthException.FromInvalidRequest("No scopes approved");

        if (!ParseScope(request.ApprovedScopes).All(session.MissingScopes.Contains))
            throw OAuthException.FromInvalidRequest("Invalid scopes in approval");

        var scopesToPersist = ParseScope(request.ApprovedScopes);

        await SaveConsentAsync(session.UserId!, session.ClientId, scopesToPersist, ct);
        
        session = session with { Status = AuthorizationStatus.Consented };
        await _authorizationSessionRepository.StoreAsync(session, ct);

        return new RedirectResponse
        {
            Action = "resume",
            RedirectUrl = RedirectUrlFactory.AuthorizeResume(session: sessionId)
        };
    }

    public async Task<RedirectResponse> HandleParConsentAsync(string sessionId,
                                                              ConsentDecisionRequest request,
                                                              CancellationToken ct)
    {
        var session = await GetSessionAsync(sessionId, ct);

        if (session is null)
            throw OAuthException.FromInvalidRequest("Invalid session");

        if (session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            session = session with { Status = AuthorizationStatus.Expired };

            await _authorizationSessionRepository.StoreAsync(session, ct);

            throw OAuthException.FromInvalidRequest("Expired session");
        }

        if (session.Status != AuthorizationStatus.ConsentRequired)
            throw OAuthException.FromInvalidRequest("Invalid session state for consent");

        if (session.UserId is null)
            throw OAuthException.FromAccessDenied("User not authenticated");

        var entry = await _parEntryRepository.GetAsync(session.RequestUriCode!, ct);

        if (entry is null)
            throw OAuthException.FromInvalidRequest("PAR entry missing");

        if (entry.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            session = session with { Status = AuthorizationStatus.Expired };

            await _authorizationSessionRepository.StoreAsync(session, ct);

            throw OAuthException.FromInvalidRequest("Expired session");
        }

        if (!request.Approved)
        {
            session = session with { Status = AuthorizationStatus.Aborted };

            await _authorizationSessionRepository.StoreAsync(session, ct);
            
            return BuildErrorRedirect(entry, "access_denied");
        }

        var requestedScopes = entry.Scopes.ToHashSet();

        await SaveConsentAsync(session.UserId,
                               entry.ClientId,
                               requestedScopes.ToImmutableHashSet(),
                               ct);

        session = session with { Status = AuthorizationStatus.Consented };
        await _authorizationSessionRepository.StoreAsync(session, ct);

        var requestUri = BuildParRequestUri(session.RequestUriCode!);
        return new RedirectResponse
        {
            Action = "redirect",
            RedirectUrl = RedirectUrlFactory.Authorize(requestUri)
        };
    }

    public async Task<bool> IsParConsentSession(string sessionId, CancellationToken ct)
    {
        var session = await _authorizationSessionRepository.GetAsync(sessionId, ct);

        if (session is null)
            throw OAuthException.FromInvalidRequest("Invalid session");

        return !string.IsNullOrWhiteSpace(session.RequestUriCode);
    }

    public async Task<ConsentEvaluationResult> EvaluateAsync(string userId,
                                                             string clientId,
                                                             IReadOnlySet<string> requestedScopes,
                                                             CancellationToken ct)
    {
        var consent = await _userConsentRepository.GetAsync(userId, clientId, ct);

        if (consent is null)
        {
            return new ConsentEvaluationResult
            {
                RequiresConsent = true,
                MissingScopes = requestedScopes,
            };
        }

        var missingScopes = requestedScopes.Except(consent.GrantedScopes).ToHashSet();

        var result = new ConsentEvaluationResult
        {
            RequiresConsent = missingScopes.Count == 0,
            MissingScopes = missingScopes
        };

        return result;
    }

    private async Task<AuthorizationSession?> GetSessionAsync(string sessionId, CancellationToken ct)
    {
        return await _authorizationSessionRepository.GetAsync(sessionId, ct);
    }

    private async Task SaveConsentAsync(string userId,
                                        string clientId,
                                        IReadOnlySet<string> approvedScopes,
                                        CancellationToken ct)
    {
        var existing = await _userConsentRepository.GetAsync(userId, clientId, ct);

        if (existing == null)
        {
            var consent = new UserConsent
            {
                UserId = userId,
                ClientId = clientId,
                GrantedScopes = approvedScopes.ToArray(),
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _userConsentRepository.StoreAsync(consent, ct);
            return;
        }

        var grantedScopes = existing.GrantedScopes.Union(approvedScopes).ToHashSet();

        var updatedConsent = new UserConsent
        {
            UserId = userId,
            ClientId = clientId,
            GrantedScopes = grantedScopes.ToArray(),
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _userConsentRepository.StoreAsync(updatedConsent, ct);
    }

    private static HashSet<string> ParseScope(string scope)
    {
        return scope.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToHashSet(StringComparer.Ordinal);
    }

    private static string BuildParRequestUri(string code)
    {
        return $"urn:ietf:params:oauth:request_uri:{code}";
    }

    private static string BuildErrorRedirect(AuthorizationSession session)
    {
        return QueryHelpers.AddQueryString(session.Request.RedirectUri, new Dictionary<string, string?>
        {
            ["error"] = "access_denied",
            ["state"] = session.Request.State
        });
    }

    private static RedirectResponse BuildErrorRedirect(ParEntry entry, string error)
    {
        var uri = new UriBuilder(entry.RedirectUri);

        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        query["error"] = error;

        if (!string.IsNullOrEmpty(entry.State))
            query["state"] = entry.State;

        uri.Query = query.ToString();

        return new RedirectResponse
        {
            Action = "redirect",
            RedirectUrl = uri.ToString()
        };
    }
}
