using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client.Generated;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Patterns.Services;

public class SignalEntry
{
    public string SignalKey { get; set; } = default!;
    public string? WaitWorkflowId { get; set; }
    public string? WaitTaskRefName { get; set; }
    public TaskResultStatus? SignalStatus { get; set; }
    public Dictionary<string, object>? SignalOutputData { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// Persistence abstraction for signal-based workflow coordination.
/// Consumers implement this with their own storage (EF Core, Dapper, Redis, etc.).
/// Each method performs an upsert keyed on <see cref="SignalEntry.SignalKey"/>,
/// setting only the fields relevant to that side (wait or signal) without overwriting the other.
/// </summary>
public interface ISignalStore
{
    /// <summary>
    /// Upsert the wait-side fields for the given signal key.
    /// If no row exists, create one with <paramref name="waitWorkflowId"/> and <paramref name="waitTaskRefName"/>.
    /// If a row already exists, update only these two fields (do not overwrite signal-side fields).
    /// Returns the full entry after the upsert so the caller can check if the signal slot is already filled.
    /// </summary>
    Task<SignalEntry> RegisterWaiterAsync(string signalKey, string waitWorkflowId, string waitTaskRefName, CancellationToken ct = default);

    /// <summary>
    /// Upsert the signal-side fields for the given signal key.
    /// If no row exists, create one with <paramref name="status"/> and <paramref name="outputData"/>.
    /// If a row already exists, update only these two fields (do not overwrite wait-side fields).
    /// Returns the full entry after the upsert so the caller can check if the wait slot is already filled.
    /// </summary>
    Task<SignalEntry> RegisterSignalAsync(
        string signalKey,
        TaskResultStatus status,
        Dictionary<string, object>? outputData = null,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns all entries where both the wait slot and signal slot are filled
    /// (i.e. WaitWorkflowId is not null AND SignalStatus is not null).
    /// </summary>
    Task<IReadOnlyList<SignalEntry>> GetResolvableEntriesAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns all entries where a workflow is waiting but no signal has been received yet
    /// (i.e. WaitWorkflowId is not null AND SignalStatus is null).
    /// Useful for monitoring and manual intervention when workflows are stuck.
    /// </summary>
    Task<IReadOnlyList<SignalEntry>> GetPendingWaitersAsync(CancellationToken ct = default);

    /// <summary>
    /// Deletes the entry with the given signal key.
    /// </summary>
    Task DeleteAsync(string signalKey, CancellationToken ct = default);
}
