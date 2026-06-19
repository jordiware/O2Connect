using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories.Filters;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories.InMemoryRepositories;

public class MemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _users = new();

    public Task<bool> ContainsUserAsync(string normalizedUsername, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        bool hasValue = _users.Values.Any(u => string.Equals(u.NormalizedUsername, normalizedUsername, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(hasValue);
    }

    public Task<int> CountAsync(CancellationToken ct)
    {
        return CountAsync(UserFilter.Empty, ct);
    }

    public Task<int> CountAsync(UserFilter filter, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(_users.Values.Count(filter.ToExpression().Compile()));
    }

    public Task<User?> GetAsync(string userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _users.TryGetValue(userId, out var value);

        return Task.FromResult(value);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var value = _users.Values.SingleOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(value);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var value = _users.Values.SingleOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(value);
    }

    public Task<IReadOnlyList<User>> QueryAsync(EntityPagination pagination,
                                                CancellationToken ct)
    {
        return QueryAsync(pagination, UserFilter.Empty, ct);
    }

    public Task<IReadOnlyList<User>> QueryAsync(EntityPagination pagination,
                                                UserFilter filter,
                                                CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var orderAscending = pagination.Order.Equals("asc", StringComparison.OrdinalIgnoreCase);

        var filtered = _users.Values.Where(filter.ToExpression().Compile());

        var users = pagination.SortBy switch
        {
            _ => orderAscending
                 ? filtered.OrderBy(c => c.NormalizedUsername,
                                         StringComparer.InvariantCultureIgnoreCase)
                 : filtered.OrderByDescending(c => c.NormalizedUsername,
                                                   StringComparer.InvariantCultureIgnoreCase)
        };

        var page = users.Skip((pagination.Page - 1) * pagination.PageSize)
                        .Take(pagination.PageSize)
                        .ToList();

        return Task.FromResult<IReadOnlyList<User>>(page);
    }

    public Task StoreAsync(User user, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _users[user.Id] = user;

        return Task.CompletedTask;
    }
}
