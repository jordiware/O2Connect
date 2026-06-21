using Microsoft.EntityFrameworkCore;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Persistence;

namespace O2Connect.Api.Repositories.DatabaseRepositories;

public sealed class DbAuthorizationCodeRepository : IAuthorizationCodeRepository
{
    private readonly AppDbContext _db;
    private DbSet<AuthorizationCode> _codes => _db.AuthorizationCodes;

    public DbAuthorizationCodeRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AuthorizationCode?> GetAsync(string code,
                                                   CancellationToken ct)
    {
        return await _codes.AsNoTracking()
                           .FirstOrDefaultAsync(c => string.Equals(c.Code,
                                                                   code,
                                                                   StringComparison.Ordinal), ct);
    }

    public async Task<AuthorizationCode?> RedeemAsync(string code,
                                                      CancellationToken ct)
    {
        var result = await GetAsync(code, ct);

        await _codes.Where(c => string.Equals(c.Code,
                                              code,
                                              StringComparison.Ordinal))
                    .ExecuteDeleteAsync(ct);

        return result;
    }

    public async Task RevokeForClientAsync(string clientId,
                                           CancellationToken ct)
    {
        await _codes.Where(c => string.Equals(c.ClientId,
                                              clientId,
                                              StringComparison.Ordinal))
                    .ExecuteDeleteAsync(ct);
    }

    public async Task RevokeForSubjectAndClientAsync(string subjectId,
                                                     string clientId,
                                                     CancellationToken ct)
    {
        await _codes.Where(c => string.Equals(c.ClientId,
                                              clientId,
                                              StringComparison.Ordinal)
                                && string.Equals(c.SubjectId,
                                                 subjectId,
                                                 StringComparison.Ordinal))
                    .ExecuteDeleteAsync(ct);
    }

    public async Task RevokeForSubjectAsync(string subjectId,
                                            CancellationToken ct)
    {
        await _codes.Where(c => string.Equals(c.SubjectId,
                                              subjectId,
                                              StringComparison.Ordinal))
                    .ExecuteDeleteAsync(ct);
    }

    public async Task StoreAsync(AuthorizationCode code,
                                 CancellationToken ct)
    {
        var exists = await _codes.AnyAsync(c => string.Equals(c.Code,
                                                              code.Code,
                                                              StringComparison.Ordinal), ct);
        if (exists)
        {
            _codes.Update(code);
        }
        else
        {
            await _codes.AddAsync(code, ct);
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> TryConsumeAsync(string code,
                                            CancellationToken ct)
    {
        var existing = await GetAsync(code, ct);

        if (existing is null)
            return false;

        existing = existing with { IsConsumed = true };

        _codes.Update(existing);

        await _db.SaveChangesAsync(ct);

        return existing.IsConsumed;
    }
}
