using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service;

public class WorkflowBulkService(ConductorClient client) : IWorkflowBulkService
{
    public async Task<BulkResponse> ResumeWorkflowsAsyncAsync(IEnumerable<string> workflowIds,
        CancellationToken cancellationToken = default) =>
        await client.ResumeWorkflow_1Async(workflowIds, cancellationToken);

    public async Task<BulkResponse> PauseWorkflowsAsync(IEnumerable<string> workflowIds,
        CancellationToken cancellationToken = default)
        => await client.PauseWorkflow_1Async(workflowIds, cancellationToken);

    public async Task<BulkResponse> TerminateWorkflowsAsync(IEnumerable<string> worklowIds, string? reason = null,
        CancellationToken cancellationToken = default)
        => await client.TerminateAsync(worklowIds, reason, cancellationToken);

    public async Task<BulkResponse> RetryWorkflowsAsync(IEnumerable<string> workflowIds,
        CancellationToken cancellationToken = default)
        => await client.Retry_1Async(workflowIds, cancellationToken);

    public async Task<BulkResponse> RestartWorkflowsAsync(IEnumerable<string> workflowIds,
        bool? useLatestDefinition = null,
        CancellationToken cancellationToken = default)
        => await client.Restart_1Async(workflowIds, useLatestDefinition, cancellationToken);
}