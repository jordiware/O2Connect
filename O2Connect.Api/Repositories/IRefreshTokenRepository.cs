using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetAsync(string token, CancellationToken ct);
    Task CreateAsync(RefreshToken token, CancellationToken ct);
    Task ConsumeAndCreateAsync(RefreshToken token, RefreshToken newToken, CancellationToken ct);
    Task<bool> TryConsumeAsync(string token, CancellationToken ct);
    Task<bool> IsConsumedAsync(string token, CancellationToken ct);
    Task<bool> IsSessionActiveAsync(string sessionId, CancellationToken ct);
    Task RevokeAsync(string token, CancellationToken ct);
    Task RevokeSessionAsync(string sessionId, CancellationToken ct);
    Task RevokeSubjectAsync(string subjectId, CancellationToken ct);
    Task<RefreshToken?> RotateAsync(string token, RefreshToken newToken, CancellationToken ct);
}
