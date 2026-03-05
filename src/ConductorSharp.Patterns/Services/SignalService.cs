using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Patterns.Workflows;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;
using TaskStatus = ConductorSharp.Client.Generated.TaskStatus;

namespace ConductorSharp.Patterns.Services;

public class SignalService(ISignalStore signalStore, IWorkflowService workflowService, ITaskService taskService, ILogger<SignalService> logger)
    : ISignalService
{
    public async Task SendSignalAsync(
        string signalKey,
        TaskResultStatus status,
        Dictionary<string, object>? outputData = null,
        CancellationToken ct = default
    )
    {
        var entry = await signalStore.RegisterSignalAsync(signalKey, status, outputData, ct);

        if (entry.WaitWorkflowId is not null)
        {
            await TryCompleteWaitTaskAsync(entry, ct);
        }
    }

    private async Task TryCompleteWaitTaskAsync(SignalEntry entry, CancellationToken ct)
    {
        try
        {
            var workflow = await workflowService.GetExecutionStatusAsync(entry.WaitWorkflowId!, includeTasks: true, cancellationToken: ct);

            var waitTask = workflow.Tasks?.FirstOrDefault(t => t.ReferenceTaskName == entry.WaitTaskRefName && t.Status == TaskStatus.IN_PROGRESS);

            if (waitTask is null)
            {
                logger.LogDebug(
                    "WAIT task {TaskRefName} not yet in progress for workflow {WorkflowId}, sweeper will handle signal {SignalKey}",
                    entry.WaitTaskRefName,
                    entry.WaitWorkflowId,
                    entry.SignalKey
                );
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

            logger.LogInformation("Completed WAIT task for signal {SignalKey} in workflow {WorkflowId}", entry.SignalKey, entry.WaitWorkflowId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            logger.LogWarning(
                "Workflow {WorkflowId} not found while completing signal {SignalKey}, sweeper will retry",
                entry.WaitWorkflowId,
                entry.SignalKey
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to complete WAIT task inline for signal {SignalKey}, sweeper will handle it", entry.SignalKey);
        }
    }
}
