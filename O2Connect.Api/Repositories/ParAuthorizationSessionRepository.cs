using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories;

public interface IParAuthorizationSessionRepository
{
    Task<ParAuthorizationSession?> GetAsync(string sessionId, CancellationToken ct);
    Task<ParAuthorizationSession?> GetFromRequestUriAsync(string requestUri, CancellationToken ct);
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

    public Task<ParAuthorizationSession?> GetFromRequestUriAsync(string requestUri, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var entry = _entries.Values.SingleOrDefault(entry => string.Equals(requestUri, entry.RedirectUri));
        return Task.FromResult(entry);
    }

    public Task StoreAsync(ParAuthorizationSession parAuthSession, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _entries[parAuthSession.SessionId] = parAuthSession;
        return Task.CompletedTask;
    }

}
