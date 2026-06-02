using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;

namespace O2Connect.Api.Services;

public interface IConsentService
{
    Task<ConsentEvaluationResult> EvaluateAsync(string userId,
                                                string clientId,
                                                HashSet<string> requestedScopes,
                                                CancellationToken ct);
    Task<AuthorizationSession?> GetSessionAsync(string sessionId,
                                                CancellationToken ct);
    Task SaveConsentAsync(string userId,
                          string clientId,
                          HashSet<string> approvedScopes,
                          CancellationToken ct);
}

public class ConsentService : IConsentService
{
    private readonly IUserConsentRepository _userConsentRepository;
    private readonly IAuthorizationSessionRepository _authorizationSessionRepository;

    public ConsentService(
        IUserConsentRepository userConsentRepository,
        IAuthorizationSessionRepository authorizationSessionRepository)
    {
        _userConsentRepository = userConsentRepository;
        _authorizationSessionRepository = authorizationSessionRepository;
    }

    public async Task<ConsentEvaluationResult> EvaluateAsync(string userId,
                                                             string clientId,
                                                             HashSet<string> requestedScopes,
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

        var missing = requestedScopes.Except(existing.GrantedScopes).ToHashSet();

        return new ConsentEvaluationResult
        {
            RequiresConsent = missing.Any(),
            MissingScopes = missing
        };
    }

    public async Task<AuthorizationSession?> GetSessionAsync(string sessionId, CancellationToken ct)
    {
        var session = await _authorizationSessionRepository.GetAsync(sessionId, ct);
        return session;
    }

    public async Task SaveConsentAsync(string userId,
                                       string clientId,
                                       HashSet<string> approvedScopes,
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

        existing.GrantedScopes.UnionWith(approvedScopes);

        var updatedConsent = new UserConsent
        {
            Id = existing.Id,
            UserId = userId,
            ClientId = clientId,
            GrantedScopes = approvedScopes,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _userConsentRepository.StoreAsync(updatedConsent, ct);
    }
}
