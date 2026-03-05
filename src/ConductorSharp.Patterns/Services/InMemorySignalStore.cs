using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client.Generated;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Patterns.Services;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="ISignalStore"/>.
/// Suitable for development, testing, and single-instance deployments.
/// For production multi-instance deployments, use a persistent store (database, Redis, etc.).
/// </summary>
public class InMemorySignalStore : ISignalStore
{
    private static readonly ConcurrentDictionary<string, SignalEntry> _store = new();

    public Task<SignalEntry> RegisterWaiterAsync(string signalKey, string waitWorkflowId, string waitTaskRefName, CancellationToken ct = default)
    {
        var entry = _store.AddOrUpdate(
            signalKey,
            _ =>
                new SignalEntry
                {
                    SignalKey = signalKey,
                    WaitWorkflowId = waitWorkflowId,
                    WaitTaskRefName = waitTaskRefName,
                    CreatedAt = DateTimeOffset.UtcNow
                },
            (_, existing) =>
            {
                existing.WaitWorkflowId = waitWorkflowId;
                existing.WaitTaskRefName = waitTaskRefName;
                return existing;
            }
        );

        return Task.FromResult(entry);
    }

    public Task<SignalEntry> RegisterSignalAsync(
        string signalKey,
        TaskResultStatus status,
        Dictionary<string, object>? outputData = null,
        CancellationToken ct = default
    )
    {
        var entry = _store.AddOrUpdate(
            signalKey,
            _ =>
                new SignalEntry
                {
                    SignalKey = signalKey,
                    SignalStatus = status,
                    SignalOutputData = outputData,
                    CreatedAt = DateTimeOffset.UtcNow
                },
            (_, existing) =>
            {
                existing.SignalStatus = status;
                existing.SignalOutputData = outputData;
                return existing;
            }
        );

        return Task.FromResult(entry);
    }

    public Task<IReadOnlyList<SignalEntry>> GetResolvableEntriesAsync(CancellationToken ct = default)
    {
        var resolvable = _store.Values.Where(e => e.WaitWorkflowId is not null && e.SignalStatus is not null).ToList();

        return Task.FromResult<IReadOnlyList<SignalEntry>>(resolvable);
    }

    public Task<IReadOnlyList<SignalEntry>> GetPendingWaitersAsync(CancellationToken ct = default)
    {
        var pending = _store.Values.Where(e => e.WaitWorkflowId is not null && e.SignalStatus is null).ToList();

        return Task.FromResult<IReadOnlyList<SignalEntry>>(pending);
    }

    public Task DeleteAsync(string signalKey, CancellationToken ct = default)
    {
        _store.TryRemove(signalKey, out _);
        return Task.CompletedTask;
    }
}
