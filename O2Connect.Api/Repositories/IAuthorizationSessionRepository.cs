using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Repositories;

public interface IAuthorizationSessionRepository
{
    Task<AuthorizationSession?> GetAsync(string id, CancellationToken ct);
    Task<AuthorizationSession?> GetFromRequestUriCodeAsync(string code, CancellationToken ct);
    Task StoreAsync(AuthorizationSession session, CancellationToken ct);
    Task<AuthorizationSession?> TryConsumeAsync(string id, CancellationToken ct);
}
