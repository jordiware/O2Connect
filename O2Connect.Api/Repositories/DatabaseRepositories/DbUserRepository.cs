using Microsoft.EntityFrameworkCore;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Persistence;
using O2Connect.Api.Repositories.Filters;

namespace O2Connect.Api.Repositories.DatabaseRepositories;

public sealed class DbUserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public DbUserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ContainsUserAsync(string normalizedUsername,
                                              CancellationToken ct)
    {
        return await _db.Users.AsNoTracking()
                              .AnyAsync(u => u.NormalizedUsername == normalizedUsername, ct);
    }

    public async Task<int> CountAsync(CancellationToken ct)
    {
        return await _db.Users.AsNoTracking()
                              .CountAsync(ct);
    }

    public async Task<int> CountAsync(UserFilter filter,
                                      CancellationToken ct)
    {
        return await _db.Users.AsNoTracking()
                              .Where(filter.ToExpression())
                              .CountAsync(ct);
    }

    public async Task<User?> GetAsync(string userId,
                                      CancellationToken ct)
    {
        return await _db.Users.AsNoTracking()
                              .FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    public async Task<User?> GetByEmailAsync(string email,
                                             CancellationToken ct)
    {
        return await _db.Users.AsNoTracking()
                              .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<User?> GetByUsernameAsync(string username,
                                                CancellationToken ct)
    {
        return await _db.Users.AsNoTracking()
                              .FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task<IReadOnlyList<User>> QueryAsync(EntityPagination pagination,
                                                      CancellationToken ct)
    {
        return await _db.Users.AsNoTracking()
                              .AsQueryable()
                              .ApplySorting(pagination)
                              .ApplyPagination(pagination)
                              .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<User>> QueryAsync(EntityPagination pagination,
                                                      UserFilter filter,
                                                      CancellationToken ct)
    {
        return await _db.Users.AsNoTracking()
                              .AsQueryable()
                              .Where(filter.ToExpression())
                              .ApplySorting(pagination)
                              .ApplyPagination(pagination)
                              .ToListAsync(ct);
    }

    public async Task StoreAsync(User user,
                                 CancellationToken ct)
    {
        var exists = await _db.Users.AnyAsync(u => u.Id == user.Id, ct);

        if (exists)
        {
            _db.Users.Update(user);
        }
        else
        {
            await _db.Users.AddAsync(user, ct);
        }

        await _db.SaveChangesAsync(ct);
    }
}
