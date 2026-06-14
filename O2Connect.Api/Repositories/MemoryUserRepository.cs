using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories;

public class MemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _users = new();

    public Task<bool> ContainsUserAsync(string normalizedUsername, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        bool hasValue = _users.Values.Any(u => string.Equals(u.NormalizedUsername, normalizedUsername, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(hasValue);
    }

    public Task<User?> GetAsync(string userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _users.TryGetValue(userId, out var value);

        return Task.FromResult(value);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var value = _users.Values.SingleOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(value);
    }

    public Task StoreAsync(User user, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _users[user.Id] = user;

        return Task.CompletedTask;
    }
}
