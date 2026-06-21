using Microsoft.EntityFrameworkCore;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Persistence;

namespace O2Connect.Api.Repositories.DatabaseRepositories;

public sealed class DbParEntryRepository : IParEntryRepository
{
    private readonly AppDbContext _db;

    public DbParEntryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ParEntry?> GetAsync(string code, CancellationToken ct)
    {
        return await _db.ParEntries.AsNoTracking()
                                   .FirstOrDefaultAsync(p => string.Equals(p.RequestUriCode,
                                                                           code,
                                                                           StringComparison.Ordinal));
    }

    public async Task StoreAsync(ParEntry parEntry, CancellationToken ct)
    {
        var exists = await _db.ParEntries.AnyAsync(p => string.Equals(p.RequestUriCode,
                                                                      parEntry.RequestUriCode,
                                                                      StringComparison.Ordinal));
        if (exists)
        {
            _db.ParEntries.Update(parEntry);
        }
        else
        {
            await _db.ParEntries.AddAsync(parEntry, ct);
        }

        await _db.SaveChangesAsync(ct);
    }
}
