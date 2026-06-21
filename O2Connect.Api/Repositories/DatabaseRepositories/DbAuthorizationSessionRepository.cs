using Microsoft.EntityFrameworkCore;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Persistence;

namespace O2Connect.Api.Repositories.DatabaseRepositories;

public sealed class DbAuthorizationSessionRepository : IAuthorizationSessionRepository
{
    private readonly AppDbContext _db;
    private DbSet<AuthorizationSession> _sessions => _db.AuthorizationSessions;

    public DbAuthorizationSessionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AuthorizationSession?> ConsumeAsync(string id,
                                                          CancellationToken ct)
    {
        var session = await _sessions.FirstOrDefaultAsync(s => string.Equals(s.SessionId,
                                                                             id,
                                                                             StringComparison.Ordinal), ct);
        await _sessions.Where(s => string.Equals(s.SessionId,
                                                 id,
                                                 StringComparison.Ordinal))
                       .ExecuteDeleteAsync(ct);

        return session;
    }

    public async Task<AuthorizationSession?> GetAsync(string id,
                                                      CancellationToken ct)
    {
        return await _sessions.AsNoTracking()
                              .FirstOrDefaultAsync(s => string.Equals(s.SessionId,
                                                                      id,
                                                                      StringComparison.Ordinal), ct);
    }

    public async Task<AuthorizationSession?> GetFromRequestUriCodeAsync(string code,
                                                                        CancellationToken ct)
    {
        return await _sessions.AsNoTracking()
                              .FirstOrDefaultAsync(s => string.Equals(s.RequestUriCode,
                                                                      code,
                                                                      StringComparison.Ordinal), ct);
    }

    public async Task StoreAsync(AuthorizationSession session,
                                 CancellationToken ct)
    {
        var exists = await _sessions.AnyAsync(s => string.Equals(s.SessionId,
                                                                 session.SessionId,
                                                                 StringComparison.Ordinal), ct);
        if (exists)
        {
            _sessions.Update(session);
        }
        else
        {
            await _sessions.AddAsync(session, ct);
        }

        await _db.SaveChangesAsync(ct);
    }
}
