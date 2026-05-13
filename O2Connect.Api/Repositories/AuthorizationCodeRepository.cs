using O2Connect.Api.Models;

namespace O2Connect.Api.Repositories;

public interface IAuthorizationCodeRepository
{
    Task StoreAsync(AuthorizationCode code);
    Task<AuthorizationCode?> GetAsync(string code);
    Task<AuthorizationCode?> RedeemAsync(string code);
    Task RemoveAsync(string code);
}

public class InMemoryAuthorizationCodeRepository : IAuthorizationCodeRepository
{
    private readonly Dictionary<string, AuthorizationCode> _codes = new();

    public Task StoreAsync(AuthorizationCode code)
    {
        _codes[code.Code] = code;
        return Task.CompletedTask;
    }

    public Task<AuthorizationCode?> GetAsync(string code)
    {
        _codes.TryGetValue(code, out var value);
        return Task.FromResult(value);
    }

    public async Task<AuthorizationCode?> RedeemAsync(string code)
    {
        _codes.TryGetValue(code, out var value);
        await RemoveAsync(code);
        return value;
    }

    public Task RemoveAsync(string code)
    {
        _codes.Remove(code);
        return Task.CompletedTask;
    }
}
