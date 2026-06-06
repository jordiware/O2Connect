using O2Connect.Api.Models.Store;
using System.Collections.Concurrent;

namespace O2Connect.Api.Repositories;

public interface IParEntryRepository
{
    Task<ParEntry> ConsumeParAsync(string requestUri, CancellationToken ct);
    Task<ParEntry?> GetAsync(string requestUri, CancellationToken ct);
    Task StoreAsync(string requestUri, ParEntry parEntry, CancellationToken ct);
}

public class InMemoryParEntryRepository : IParEntryRepository
{
    private static readonly ConcurrentDictionary<string, ParEntry> _entries = new();

    public Task<ParEntry> ConsumeParAsync(string requestUri, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _entries.TryGetValue(requestUri, out var parEntry);

        if (parEntry is null)
            throw new InvalidOperationException("invalid_request_uri");

        if (parEntry.Status != ParStatus.Created)
            throw new InvalidOperationException("request_uri_already_used");

        if (parEntry.ExpiresAt < DateTimeOffset.UtcNow)
        {
            _entries[requestUri] = parEntry with { Status = ParStatus.Expired };
            throw new InvalidOperationException("request_uri_expired");
        }

        _entries[requestUri] = parEntry with { Status = ParStatus.Consumed };

        return Task.FromResult(_entries[requestUri]);
    }

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
