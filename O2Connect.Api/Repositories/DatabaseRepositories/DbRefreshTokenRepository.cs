using Microsoft.EntityFrameworkCore;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Persistence;
using static O2Connect.Api.Models.Scopes;

namespace O2Connect.Api.Repositories.DatabaseRepositories;

public sealed class DbRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _db;

    private DbSet<RefreshToken> _tokens => _db.RefreshTokens;

    public DbRefreshTokenRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task ConsumeAndCreateAsync(RefreshToken? token,
                                            RefreshToken newToken,
                                            CancellationToken ct)
    {
        if (token is not null)
        {
            token = token with
            {
                ConsumedAt = DateTimeOffset.UtcNow,
                ReplacedByToken = newToken.Token
            };

            _tokens.Update(token);
        }

        _tokens.Add(newToken);

        await _db.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetAsync(string token,
                                              CancellationToken ct)
    {
        return await _tokens.AsNoTracking()
                            .FirstOrDefaultAsync(t => t.Token == token, ct);
    }

    public async Task<bool> IsSessionActiveAsync(string sessionId,
                                                 CancellationToken ct)
    {
        return await _tokens.AsNoTracking()
                            .AnyAsync(t => string.Equals(t.SessionId, sessionId, StringComparison.Ordinal)
                                           && !t.IsConsumed
                                           && !t.IsRevoked
                                           && t.ExpiresAt > DateTimeOffset.UtcNow);
    }

    public async Task RevokeClientAsync(string clientId,
                                        CancellationToken ct)
    {
        var utcNow = DateTimeOffset.UtcNow;

        await _tokens.Where(t => string.Equals(t.ClientId, clientId, StringComparison.Ordinal))
                     .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAt, t => utcNow));
    }

    public async Task RevokeForSubjectAndClientAsync(string subjectId,
                                                     string clientId,
                                                     CancellationToken ct)
    {
        var utcNow = DateTimeOffset.UtcNow;

        await _tokens.Where(t => string.Equals(t.Subject, subjectId, StringComparison.Ordinal)
                                 && string.Equals(t.ClientId, clientId, StringComparison.Ordinal))
                     .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAt, t => utcNow));
    }

    public async Task RevokeSessionAsync(string sessionId,
                                         CancellationToken ct)
    {
        var utcNow = DateTimeOffset.UtcNow;

        await _tokens.Where(t => string.Equals(t.SessionId, sessionId, StringComparison.Ordinal))
                     .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAt, t => utcNow));
    }

    public async Task RevokeSubjectAsync(string subjectId,
                                         CancellationToken ct)
    {
        var utcNow = DateTimeOffset.UtcNow;

        await _tokens.Where(t => string.Equals(t.Subject, subjectId, StringComparison.Ordinal))
                     .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAt, t => utcNow));
    }

    public async Task<RefreshToken?> RotateAsync(string token,
                                                 RefreshToken newToken,
                                                 CancellationToken ct)
    {
        var existing = await GetAsync(token, ct);

        if (existing is not null)
        {
            existing = existing with
            {
                ConsumedAt = DateTimeOffset.UtcNow,
                ReplacedByToken = newToken.Token
            };

            _tokens.Update(existing);
        }

        _tokens.Add(newToken);

        await _db.SaveChangesAsync();

        return newToken;
    }
}
