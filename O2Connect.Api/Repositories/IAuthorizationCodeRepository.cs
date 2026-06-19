using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Repositories;

public interface IAuthorizationCodeRepository
{
    Task StoreAsync(AuthorizationCode code, CancellationToken ct);
    Task<AuthorizationCode?> GetAsync(string code, CancellationToken ct);
    Task<AuthorizationCode?> RedeemAsync(string code, CancellationToken ct);
    Task<bool> TryConsumeAsync(string code, CancellationToken ct);
    Task RemoveAsync(string code, CancellationToken ct);
    Task RevokeForClientAsync(string clientId, CancellationToken ct);
    Task RevokeForSubjectAsync(string subjectId, CancellationToken ct);
    Task RevokeForSubjectAndClientAsync(string subjectId, string clientId, CancellationToken ct);
}
