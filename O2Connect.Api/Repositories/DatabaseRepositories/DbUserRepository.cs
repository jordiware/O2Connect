using Microsoft.EntityFrameworkCore;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Persistence;
using O2Connect.Api.Repositories.Filters;
using System.Reflection;

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
        return await _db.Users.CountAsync(ct);
    }

    public async Task<int> CountAsync(UserFilter filter,
                                      CancellationToken ct)
    {
        var query = _db.Users.AsQueryable()
                             .Where(filter.ToExpression());

        return await query.CountAsync(ct);
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
        var query = _db.Users.AsNoTracking()
                             .AsQueryable();

        query = ApplySorting(query, pagination);
        query = ApplyPagination(query, pagination);

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<User>> QueryAsync(EntityPagination pagination,
                                                      UserFilter filter,
                                                      CancellationToken ct)
    {
        var query = _db.Users.AsNoTracking()
                             .Where(filter.ToExpression());

        query = ApplySorting(query, pagination);
        query = ApplyPagination(query, pagination);

        return await query.ToListAsync(ct);
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

    private static IQueryable<User> ApplyPagination(IQueryable<User> query,
                                                    EntityPagination pagination)
    {
        var skip = (pagination.Page - 1) * pagination.PageSize;

        return query.Skip(skip).Take(pagination.PageSize);
    }

    private static IQueryable<User> ApplySorting(IQueryable<User> query,
                                                 EntityPagination pagination)
    {
        var property = typeof(User).GetProperty(pagination.SortBy,
                                                BindingFlags.IgnoreCase |
                                                BindingFlags.Public |
                                                BindingFlags.Instance);

        if (property is null)
        {
            return query.OrderBy(u => u.Id);
        }

        return pagination.Order.Equals("desc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderByDescending(e => EF.Property<object>(e, property.Name))
            : query.OrderBy(e => EF.Property<object>(e, property.Name));
    }
}
