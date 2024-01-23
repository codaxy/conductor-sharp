using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service;

public class WorkflowBulkService(ConductorClient client) : IWorkflowBulkService
{
    public async Task<BulkResponse> ResumeAsync(IEnumerable<string> workflowIds,
        CancellationToken cancellationToken = default) =>
        await client.ResumeWorkflow_1Async(workflowIds, cancellationToken);

    public async Task<BulkResponse> PauseAsync(IEnumerable<string> workflowIds,
        CancellationToken cancellationToken = default)
        => await client.PauseWorkflow_1Async(workflowIds, cancellationToken);

    public async Task<BulkResponse> TerminateAsync(IEnumerable<string> worklowIds, string? reason = null,
        CancellationToken cancellationToken = default)
        => await client.TerminateAsync(worklowIds, reason, cancellationToken);

    public async Task<BulkResponse> RetryAsync(IEnumerable<string> workflowIds,
        CancellationToken cancellationToken = default)
        => await client.Retry_1Async(workflowIds, cancellationToken);

    public async Task<BulkResponse> RestartAsync(IEnumerable<string> workflowIds,
        bool? useLatestDefinition = null,
        CancellationToken cancellationToken = default)
        => await client.Restart_1Async(workflowIds, useLatestDefinition, cancellationToken);
}