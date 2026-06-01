using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Repositories;

public interface IAuthorizationSessionRepository
{
    Task StoreAsync(AuthorizationSession session, CancellationToken ct);
    Task<AuthorizationSession?> GetAsync(string id, CancellationToken ct);
    Task DeleteAsync(string id, CancellationToken ct);
}

public class InMemoryAuthorizationSessionRepository : IAuthorizationSessionRepository
{
    public Task DeleteAsync(string id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<AuthorizationSession?> GetAsync(string id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task StoreAsync(AuthorizationSession session, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
