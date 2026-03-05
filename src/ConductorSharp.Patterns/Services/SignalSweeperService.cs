using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;
using TaskStatus = ConductorSharp.Client.Generated.TaskStatus;

namespace ConductorSharp.Patterns.Services;

/// <summary>
/// Generic background sweeper that reconciles signal entries where both the wait and signal
/// slots are filled but the WAIT task hasn't been completed yet (narrow race condition).
/// This service is fully generic — it never needs to change when new signal types are added.
/// </summary>
public class SignalSweeperService(IServiceScopeFactory scopeFactory, ILogger<SignalSweeperService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SweepAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error during signal sweep cycle");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task SweepAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var signalStore = scope.ServiceProvider.GetRequiredService<ISignalStore>();
        var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
        var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

        var entries = await signalStore.GetResolvableEntriesAsync(ct);

        if (entries.Count == 0)
            return;

        logger.LogDebug("Signal sweeper found {Count} resolvable entries", entries.Count);

        foreach (var entry in entries)
        {
            try
            {
                await TryResolveEntryAsync(entry, signalStore, workflowService, taskService, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Failed to resolve signal entry {SignalKey}, will retry next cycle", entry.SignalKey);
            }
        }
    }

    private async Task TryResolveEntryAsync(
        SignalEntry entry,
        ISignalStore signalStore,
        IWorkflowService workflowService,
        ITaskService taskService,
        CancellationToken ct
    )
    {
        Workflow workflow;
        try
        {
            workflow = await workflowService.GetExecutionStatusAsync(entry.WaitWorkflowId!, includeTasks: true, cancellationToken: ct);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            logger.LogWarning(
                "Workflow {WorkflowId} for signal {SignalKey} no longer exists, cleaning up stale entry",
                entry.WaitWorkflowId,
                entry.SignalKey
            );
            await signalStore.DeleteAsync(entry.SignalKey, ct);
            return;
        }

        var waitTask = workflow.Tasks?.FirstOrDefault(t => t.ReferenceTaskName == entry.WaitTaskRefName && t.Status == TaskStatus.IN_PROGRESS);

        if (waitTask is null)
        {
            if (
                workflow.Status == WorkflowStatus.COMPLETED
                || workflow.Status == WorkflowStatus.TERMINATED
                || workflow.Status == WorkflowStatus.FAILED
                || workflow.Status == WorkflowStatus.TIMED_OUT
            )
            {
                logger.LogWarning(
                    "Workflow {WorkflowId} for signal {SignalKey} is in terminal state {Status}, cleaning up",
                    entry.WaitWorkflowId,
                    entry.SignalKey,
                    workflow.Status
                );
                await signalStore.DeleteAsync(entry.SignalKey, ct);
            }
            else
            {
                logger.LogDebug(
                    "WAIT task {TaskRefName} not yet in progress for signal {SignalKey}, skipping until next cycle",
                    entry.WaitTaskRefName,
                    entry.SignalKey
                );
            }
            return;
        }

        await taskService.UpdateAsync(
            new TaskResult
            {
                Status = entry.SignalStatus,
                WorkflowInstanceId = workflow.WorkflowId,
                TaskId = waitTask.TaskId,
                OutputData =
                    entry.SignalOutputData as IDictionary<string, object>
                    ?? entry.SignalOutputData?.ToDictionary(kv => kv.Key, kv => kv.Value)
                    ?? new Dictionary<string, object>(),
                ReasonForIncompletion = entry.SignalStatus == TaskResultStatus.FAILED ? $"Signal '{entry.SignalKey}' reported failure" : null
            },
            ct
        );

        await signalStore.DeleteAsync(entry.SignalKey, ct);

        logger.LogInformation(
            "Sweeper resolved signal {SignalKey} by completing WAIT task in workflow {WorkflowId}",
            entry.SignalKey,
            entry.WaitWorkflowId
        );
    }
}
