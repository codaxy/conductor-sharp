using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service;

public interface IWorkflowBulkService
{
    /// <summary>
    /// Resume the list of workflows
    /// </summary>
    Task<BulkResponse> ResumeAsync(IEnumerable<string> workflowIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pause the list of workflows
    /// </summary>
    Task<BulkResponse> PauseAsync(IEnumerable<string> workflowIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminate workflows execution
    /// </summary>
    Task<BulkResponse> TerminateAsync(IEnumerable<string> worklowIds, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retry the last failed task for each workflow from the list
    /// </summary>
    Task<BulkResponse> RetryAsync(IEnumerable<string> workflowIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restart the list of completed workflow
    /// </summary>
    Task<BulkResponse> RestartAsync(
        IEnumerable<string> workflowIds,
        bool? useLatestDefinitions = null,
        CancellationToken cancellationToken = default
    );
}
