using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service;

public class WorkflowBulkService(HttpClient client) : IWorkflowBulkService
{
    private readonly ConductorClient _client = new(client);

    public async Task<BulkResponse> ResumeAsync(IEnumerable<string> workflowIds, CancellationToken cancellationToken = default) =>
        await _client.ResumeWorkflow_1Async(workflowIds, cancellationToken);

    public async Task<BulkResponse> PauseAsync(IEnumerable<string> workflowIds, CancellationToken cancellationToken = default) =>
        await _client.PauseWorkflow_1Async(workflowIds, cancellationToken);

    public async Task<BulkResponse> TerminateAsync(
        IEnumerable<string> worklowIds,
        string? reason = null,
        CancellationToken cancellationToken = default
    ) => await _client.TerminateAsync(reason, worklowIds, cancellationToken);

    public async Task<BulkResponse> RetryAsync(IEnumerable<string> workflowIds, CancellationToken cancellationToken = default) =>
        await _client.Retry_1Async(workflowIds, cancellationToken);

    public async Task<BulkResponse> RestartAsync(
        IEnumerable<string> workflowIds,
        bool? useLatestDefinition = null,
        CancellationToken cancellationToken = default
    ) => await _client.Restart_1Async(useLatestDefinition, workflowIds, cancellationToken);
}
