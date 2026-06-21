using Microsoft.EntityFrameworkCore;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Persistence;

namespace O2Connect.Api.Repositories.DatabaseRepositories;

public sealed class DbDeviceAuthorizationRepository : IDeviceAuthorizationRepository
{
    private readonly AppDbContext _db;

    public DbDeviceAuthorizationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DeviceAuthorization?> GetAsync(string deviceCodeHash,
                                                     CancellationToken ct)
    {
        return await _db.DeviceAuthorizations.AsNoTracking()
            .FirstOrDefaultAsync(p => string.Equals(p.DeviceCodeHash,
                                                    deviceCodeHash,
                                                    StringComparison.Ordinal), ct);
    }

    public async Task<DeviceAuthorization?> GetByUserCodeAsync(string userCodeHash,
                                                               CancellationToken ct)
    {
        return await _db.DeviceAuthorizations.AsNoTracking()
            .FirstOrDefaultAsync(p => string.Equals(p.UserCodeHash,
                                                    userCodeHash,
                                                    StringComparison.Ordinal), ct);
    }

    public async Task StoreAsync(DeviceAuthorization authorization,
                                 CancellationToken ct)
    {
        var exists = await _db.DeviceAuthorizations.AnyAsync(p => string.Equals(p.DeviceCodeHash,
                                                                                authorization.DeviceCodeHash,
                                                                                StringComparison.Ordinal), ct);
        if (exists)
        {
            _db.DeviceAuthorizations.Update(authorization);
        }
        else
        {
            await _db.DeviceAuthorizations.AddAsync(authorization, ct);
        }

        await _db.SaveChangesAsync(ct);
    }
}
