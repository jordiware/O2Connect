using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories;

public interface IParEntryRepository
{
    Task<ParEntry?> GetAsync(string requestUri, CancellationToken ct);
    Task StoreAsync(string requestUri, ParEntry parEntry, CancellationToken ct);
}

public class InMemoryParEntryRepository : IParEntryRepository
{
    private static readonly ConcurrentDictionary<string, ParEntry> _entries = new();

    public Task<ParEntry?> GetAsync(string requestUri, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _entries.TryGetValue(requestUri, out var parEntry);
        return Task.FromResult(parEntry);
    }

    public Task StoreAsync(string requestUri, ParEntry parEntry, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _entries[requestUri] = parEntry;
        return Task.CompletedTask;
    }
}
