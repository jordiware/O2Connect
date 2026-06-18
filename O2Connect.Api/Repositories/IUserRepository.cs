using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories.Filters;

namespace O2Connect.Api.Repositories;

public interface IUserRepository
{
    Task<bool> ContainsUserAsync(string normalizedUsername,
                                 CancellationToken ct);
    Task<int> CountAsync(UserFilter filter,
                         CancellationToken ct);
    Task<int> CountAsync(CancellationToken ct);
    Task<User?> GetAsync(string userId,
                         CancellationToken ct);
    Task<User?> GetByEmailAsync(string email,
                                CancellationToken ct);
    Task<User?> GetByUsernameAsync(string username,
                                   CancellationToken ct);
    Task<IReadOnlyList<User>> QueryAsync(EntityPagination pagination,
                                         UserFilter filter,
                                         CancellationToken ct);
    Task<IReadOnlyList<User>> QueryAsync(EntityPagination pagination,
                                         CancellationToken ct);
    Task StoreAsync(User user,
                    CancellationToken ct);
}
