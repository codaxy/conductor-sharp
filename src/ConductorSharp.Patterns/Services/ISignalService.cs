using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client.Generated;
using ConductorSharp.Patterns.Workflows;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Patterns.Services;

/// <summary>
/// Service for sending completion/failure signals to waiting workflows.
/// Inject this from workers, API controllers, background services, or anywhere
/// you need to unblock a workflow that is paused on a <see cref="SignalWait"/> sub-workflow.
/// </summary>
public interface ISignalService
{
    /// <summary>
    /// Sends a signal for the given key, unblocking any workflow waiting on it.
    /// The signal is persisted durably first, then an optimistic fast-path attempts
    /// to complete the WAIT task immediately. If that fails (e.g. timing race),
    /// the background sweeper will reconcile.
    /// </summary>
    Task SendSignalAsync(string signalKey, TaskResultStatus status, Dictionary<string, object>? outputData = null, CancellationToken ct = default);
}
