using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories;

public interface IParAuthorizationSessionRepository
{
    Task<ParAuthorizationSession?> GetAsync(string sessionId, CancellationToken ct);
    Task StoreAsync(ParAuthorizationSession parAuthSession, CancellationToken ct);
}

public class InMemoryParAuthorizationSessionRepository : IParAuthorizationSessionRepository
{
    private static readonly ConcurrentDictionary<string, ParAuthorizationSession> _entries = new();

    public Task<ParAuthorizationSession?> GetAsync(string sessionId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _entries.TryGetValue(sessionId, out var parAuthSession);
        return Task.FromResult(parAuthSession);
    }

    public Task StoreAsync(ParAuthorizationSession parAuthSession, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _entries[parAuthSession.SessionId] = parAuthSession;
        return Task.CompletedTask;
    }

}
