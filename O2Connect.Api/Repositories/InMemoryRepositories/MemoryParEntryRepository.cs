using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories.InMemoryRepositories;

public class MemoryParEntryRepository : IParEntryRepository
{
    private static readonly ConcurrentDictionary<string, ParEntry> _entries = new();

    public Task<ParEntry?> GetAsync(string code, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _entries.TryGetValue(code, out var parEntry);
        return Task.FromResult(parEntry);
    }

    public Task StoreAsync(ParEntry parEntry, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _entries[parEntry.RequestUriCode] = parEntry;
        return Task.CompletedTask;
    }
}
