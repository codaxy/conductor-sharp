using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service;

public interface IWorkflowBulkService
{
    /// <summary>
    /// Resume the list of workflows
    /// </summary>
    Task<BulkResponse> ResumeWorkflowsAsyncAsync(IEnumerable<string> workflowIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pause the list of workflows
    /// </summary>
    Task<BulkResponse> PauseWorkflowsAsync(IEnumerable<string> workflowIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminate workflows execution
    /// </summary>
    Task<BulkResponse> TerminateWorkflowsAsync(IEnumerable<string> worklowIds, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retry the last failed task for each workflow from the list
    /// </summary>
    Task<BulkResponse> RetryWorkflowsAsync(IEnumerable<string> workflowIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restart the list of completed workflow
    /// </summary>
    Task<BulkResponse> RestartWorkflowsAsync(
        IEnumerable<string> workflowIds,
        bool? useLatestDefinitions = null,
        CancellationToken cancellationToken = default
    );
}
