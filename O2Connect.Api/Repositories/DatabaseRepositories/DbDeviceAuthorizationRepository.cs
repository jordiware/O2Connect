using Microsoft.EntityFrameworkCore;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Persistence;

namespace O2Connect.Api.Repositories.DatabaseRepositories;

public sealed class DbDeviceAuthorizationRepository : IDeviceAuthorizationRepository
{
    private readonly AppDbContext _db;
    private DbSet<DeviceAuthorization> _authorizations => _db.DeviceAuthorizations;

    public DbDeviceAuthorizationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DeviceAuthorization?> GetAsync(string deviceCodeHash,
                                                     CancellationToken ct)
    {
        return await _authorizations.AsNoTracking()
                                    .FirstOrDefaultAsync(a => string.Equals(a.DeviceCodeHash,
                                                                            deviceCodeHash,
                                                                            StringComparison.Ordinal), ct);
    }

    public async Task<DeviceAuthorization?> GetByUserCodeAsync(string userCodeHash,
                                                               CancellationToken ct)
    {
        return await _authorizations.AsNoTracking()
                                    .FirstOrDefaultAsync(a => string.Equals(a.UserCodeHash,
                                                                            userCodeHash,
                                                                            StringComparison.Ordinal), ct);
    }

    public async Task StoreAsync(DeviceAuthorization authorization,
                                 CancellationToken ct)
    {
        var exists = await _authorizations.AnyAsync(p => string.Equals(p.DeviceCodeHash,
                                                                       authorization.DeviceCodeHash,
                                                                       StringComparison.Ordinal), ct);
        if (exists)
        {
            _authorizations.Update(authorization);
        }
        else
        {
            await _authorizations.AddAsync(authorization, ct);
        }

        await _db.SaveChangesAsync(ct);
    }
}
