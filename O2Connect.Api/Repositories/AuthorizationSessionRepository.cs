using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories;

public interface IAuthorizationSessionRepository
{
    Task StoreAsync(AuthorizationSession session, CancellationToken ct);
    Task<AuthorizationSession?> GetAsync(string id, CancellationToken ct);
    Task DeleteAsync(string id, CancellationToken ct);
}

public class InMemoryAuthorizationSessionRepository : IAuthorizationSessionRepository
{
    private readonly ConcurrentDictionary<string, AuthorizationSession> _sessions = new();

    public Task DeleteAsync(string id, CancellationToken ct)
    {
        _sessions.Remove(id, out _);
        return Task.CompletedTask;
    }

    public Task<AuthorizationSession?> GetAsync(string id, CancellationToken ct)
    {
        _sessions.TryGetValue(id, out var value);
        return Task.FromResult(value);
    }

    public Task StoreAsync(AuthorizationSession session, CancellationToken ct)
    {
        _sessions.TryAdd(session.Id, session);
        return Task.CompletedTask;
    }
}
