using Microsoft.EntityFrameworkCore;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Persistence;

namespace O2Connect.Api.Repositories.DatabaseRepositories;

public sealed class DbUserConsentRepository : IUserConsentRepository
{
    private readonly AppDbContext _db;

    public DbUserConsentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserConsent?> GetAsync(string userId,
                                             string clientId,
                                             CancellationToken ct)
    {
        return await _db.UserConsents.AsNoTracking()
                                     .FirstOrDefaultAsync(uc => string.Equals(uc.UserId, userId, StringComparison.Ordinal)
                                                                && string.Equals(uc.ClientId, clientId, StringComparison.Ordinal),
                                                          ct);
    }

    public async Task<IReadOnlyList<UserConsent>> GetForClientAsync(string clientId,
                                                                    CancellationToken ct)
    {
        return await _db.UserConsents.AsNoTracking()
                                     .Where(uc => string.Equals(uc.ClientId, clientId, StringComparison.Ordinal))
                                     .ToListAsync();
    }

    public async Task<IReadOnlyList<UserConsent>> GetForUserAsync(string userId,
                                                                  CancellationToken ct)
    {
        return await _db.UserConsents.AsNoTracking()
                                     .Where(uc => string.Equals(uc.UserId, userId, StringComparison.Ordinal))
                                     .ToListAsync();
    }

    public async Task RevokeAsync(string userId,
                                  string clientId,
                                  CancellationToken ct)
    {
        var entity = await _db.UserConsents.FirstOrDefaultAsync(uc => string.Equals(uc.UserId, userId, StringComparison.Ordinal)
                                                                      && string.Equals(uc.ClientId, clientId, StringComparison.Ordinal),
                                                                ct);

        if (entity is null)
            return;

        _db.UserConsents.Remove(entity);

        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeForClientAsync(string clientId,
                                           CancellationToken ct)
    {
        await _db.UserConsents.Where(uc => string.Equals(uc.ClientId, clientId, StringComparison.Ordinal))
                              .ExecuteDeleteAsync(ct);
    }

    public async Task RevokeForUserAsync(string userId,
                                         CancellationToken ct)
    {
        await _db.UserConsents.Where(uc => string.Equals(uc.UserId, userId, StringComparison.Ordinal))
                              .ExecuteDeleteAsync(ct);
    }

    public async Task StoreAsync(UserConsent consent,
                                 CancellationToken ct)
    {
        var exists = await _db.UserConsents.AnyAsync(uc => string.Equals(uc.UserId, consent.UserId, StringComparison.Ordinal)
                                                           && string.Equals(uc.ClientId, consent.ClientId, StringComparison.Ordinal),
                                                     ct);

        if (exists)
        {
            _db.UserConsents.Update(consent);
        }
        else
        {
            await _db.UserConsents.AddAsync(consent, ct);
        }

        await _db.SaveChangesAsync(ct);
    }
}
