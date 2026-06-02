using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
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
                                                ImmutableHashSet<string> requestedScopes,
                                                CancellationToken ct);
    Task<AuthorizationSession?> GetSessionAsync(string sessionId,
                                                CancellationToken ct);
    Task SaveConsentAsync(string userId,
                          string clientId,
                          ImmutableHashSet<string> approvedScopes,
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
}
