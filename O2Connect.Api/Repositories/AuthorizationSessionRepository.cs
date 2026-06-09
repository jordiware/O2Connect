using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories;

public interface IAuthorizationSessionRepository
{
    Task<AuthorizationSession?> GetAsync(string id, CancellationToken ct);
    Task<AuthorizationSession?> GetFromRequestUriCodeAsync(string code, CancellationToken ct);
    Task StoreAsync(AuthorizationSession session, CancellationToken ct);
    Task<AuthorizationSession?> TryConsumeAsync(string id, CancellationToken ct);
}

public class InMemoryAuthorizationSessionRepository : IAuthorizationSessionRepository
{
    private readonly ConcurrentDictionary<string, AuthorizationSession> _sessions = new();

    public Task<AuthorizationSession?> GetAsync(string id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        
        _sessions.TryGetValue(id, out var value);
        
        return Task.FromResult(value);
    }

    public Task<AuthorizationSession?> GetFromRequestUriCodeAsync(string code, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var value = _sessions.Values.SingleOrDefault(v => string.Equals(v.RequestUriCode, code, StringComparison.Ordinal));

        return Task.FromResult(value);
    }

    public Task StoreAsync(AuthorizationSession session, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _sessions[session.SessionId] = session;

        return Task.CompletedTask;
    }

    public Task<AuthorizationSession?> TryConsumeAsync(string id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _sessions.TryRemove(id, out var session);

        return Task.FromResult(session);
    }
}
