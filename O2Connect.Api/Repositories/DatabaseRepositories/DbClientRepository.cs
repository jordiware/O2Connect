using Microsoft.EntityFrameworkCore;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Persistence;
using O2Connect.Api.Repositories.Filters;

namespace O2Connect.Api.Repositories.DatabaseRepositories;

public sealed class DbClientRepository : IClientRepository
{
    private readonly AppDbContext _db;

    public DbClientRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> CountAsync(CancellationToken ct)
    {
        return await _db.Clients.AsNoTracking()
                                .CountAsync(ct);
    }

    public async Task<int> CountAsync(ClientFilter filter,
                                      CancellationToken ct)
    {
        return await _db.Clients.AsNoTracking()
                                .Where(filter.ToExpression())
                                .CountAsync(ct);
    }

    public async Task<Client?> GetAsync(string clientId,
                                        CancellationToken ct)
    {
        return await _db.Clients.AsNoTracking()
                                .FirstOrDefaultAsync(c => c.Id == clientId, ct);
    }

    public async Task<IReadOnlyList<Client>> QueryAsync(EntityPagination pagination,
                                                        CancellationToken ct)
    {
        return await _db.Clients.AsNoTracking()
                                .AsQueryable()
                                .ApplySorting(pagination)
                                .ApplyPagination(pagination)
                                .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Client>> QueryAsync(EntityPagination pagination,
                                                        ClientFilter filter,
                                                        CancellationToken ct)
    {
        return await _db.Clients.AsNoTracking()
                                .AsQueryable()
                                .Where(filter.ToExpression())
                                .ApplySorting(pagination)
                                .ApplyPagination(pagination)
                                .ToListAsync(ct);
    }

    public async Task StoreAsync(Client client,
                                 CancellationToken ct)
    {
        var exists = await _db.Clients.AnyAsync(c => c.Id == client.Id, ct);

        if (exists)
        {
            _db.Clients.Update(client);
        }
        else
        {
            await _db.Clients.AddAsync(client, ct);
        }

        await _db.SaveChangesAsync(ct);
    }
}
