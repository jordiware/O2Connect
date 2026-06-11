using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct);
    Task UpdateAsync(User user, CancellationToken ct);
}

public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _users = new();

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _users.TryGetValue(username, out var value);

        return Task.FromResult(value);
    }

    public Task UpdateAsync(User user, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _users[user.Username] = user;

        return Task.CompletedTask;
    }
}
