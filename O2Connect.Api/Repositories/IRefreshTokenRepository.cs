using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Repositories;

public interface IRefreshTokenRepository
{
    Task ConsumeAndCreateAsync(RefreshToken? token, RefreshToken newToken, CancellationToken ct);
    Task<RefreshToken?> GetAsync(string token, CancellationToken ct);
    Task<bool> IsSessionActiveAsync(string sessionId, CancellationToken ct);
    Task RevokeClientAsync(string clientId, CancellationToken ct);
    Task RevokeSessionAsync(string sessionId, CancellationToken ct);
    Task RevokeSubjectAsync(string subjectId, CancellationToken ct);
    Task RevokeForSubjectAndClientAsync(string subjectId, string clientId, CancellationToken ct);
    Task<RefreshToken?> RotateAsync(string token, RefreshToken newToken, CancellationToken ct);
}
