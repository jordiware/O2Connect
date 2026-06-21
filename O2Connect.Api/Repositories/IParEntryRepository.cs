using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Repositories;

public interface IParEntryRepository
{
    Task<ParEntry?> GetAsync(string code, CancellationToken ct);
    Task StoreAsync(ParEntry parEntry, CancellationToken ct);
}
