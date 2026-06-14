using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Repositories;

public interface IUserRepository
{
    Task<bool> ContainsUserAsync(string normalizedUsername, CancellationToken ct);
    Task<User?> GetAsync(string userId, CancellationToken ct);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct);
    Task StoreAsync(User user, CancellationToken ct);
}
