using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service;

public interface IWorkflowService
{
    /// <summary>
    /// Starts the decision task for a workflow
    /// </summary>
    Task DecideAsync(string workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the workflow from the system
    /// </summary>
    Task DeleteAsync(string workflowId, bool? archiveWorkflow = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists workflows for the given correlation id list
    /// </summary>
    Task<IDictionary<string, ICollection<Workflow>>> GetCorrelatedAsync(
        string name,
        IEnumerable<string> correlationIds,
        bool? includeClosed = false,
        bool? includeTasks = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Lists workflows for the given correlation id
    /// </summary>
    Task<ICollection<Workflow>> GetCorrelatedAsync(
        string name,
        string correlationId,
        bool? includeClosed = false,
        bool? includeTasks = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the workflow by workflow id
    /// </summary>
    Task<Workflow> GetExecutionStatusAsync(string workflowId, bool? includeTasks = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the uri and path of the external storage where the workflow payload is to be stored
    /// </summary>
    Task GetExternalStorageLocationAsync(string path, string operation, string payloadType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve all the running workflows
    /// </summary>
    Task<ICollection<string>> ListRunningAsync(
        string name,
        int? version,
        long? startTime = null,
        long? endTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Pauses the workflow
    /// </summary>
    Task PauseAsync(string workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reruns the workflow from a specific task
    /// </summary>
    Task<string> RerunAsync(string workflowId, RerunWorkflowRequest rerunWorkflowRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets callback times of all non-terminal SIMPLE tasks to 0
    /// </summary>
    Task ResetCallbacksAsync(string workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restarts a completed workflow
    /// </summary>
    Task RestartAsync(string workflowId, bool? useLatestDefinitions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes the workflow
    /// </summary>
    Task ResumeAsync(string workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries the last failed task
    /// </summary>
    Task RetryAsync(string workflowId, bool? resumeSubworkflowTasks = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for workflows based on payload and other parameters. Use sort options as sort=<field>:ASC|DESC e.g. sort=name&sort=workflowId:DESC. If order is not specified, defaults to ASC.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="size"></param>
    /// <param name="sort"></param>
    /// <param name="freeText"></param>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SearchResultWorkflowSummary> SearchAsync(
        int? start = null,
        int? size = null,
        string? sort = null,
        string? freeText = null,
        string? query = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Search for workflows based on task parameters. Use sort options as sort=<field>:ASC|DESC e.g. sort=name&sort=workflowId:DESC. If order is not specified, defaults to ASC.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="size"></param>
    /// <param name="sort"></param>
    /// <param name="freeText"></param>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SearchResultWorkflowSummary> SearchByTasksAsync(
        int? start = null,
        int? size = null,
        string? sort = null,
        string? freeText = null,
        string? query = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Search for workflows based on payload and other parameters. Use sort options as sort=<field>:ASC|DESC e.g. sort=name&sort=workflowId:DESC. If order is not specified, defaults to ASC.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="size"></param>
    /// <param name="sort"></param>
    /// <param name="freeText"></param>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SearchResultWorkflow> SearchV2Async(
        int? start = null,
        int? size = null,
        string? sort = null,
        string? freeText = null,
        string? query = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Search for workflows based on task parameters. Use sort options as sort=<field>:ASC|DESC e.g. sort=name&sort=workflowId:DESC. If order is not specified, defaults to ASC.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="size"></param>
    /// <param name="sort"></param>
    /// <param name="freeText"></param>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SearchResultWorkflow> SearchV2ByTasksAsync(
        int? start = null,
        int? size = null,
        string? sort = null,
        string? freeText = null,
        string? query = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Skips a given task from a current running workflow
    /// </summary>
    Task SkipTaskAsync(string workflowId, string taskReferenceName, SkipTaskRequest skipTaskRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Start a new workflow with StartWorkflowRequest, which allows task to be executed in a domain
    /// </summary>
    /// <remarks>
    /// ConductorShap: There is another Conductor API endpoint with seemingly identical functionality and operation
    /// id 'startWorkflow_1'. It seems these two achieve the same task so the second one has been left out.
    /// </remarks>
    Task<string> StartAsync(StartWorkflowRequest startWorkflowRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminate workflow execution
    /// </summary>
    Task TerminateAsync(string workflowId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Test workflow execution using mock data
    /// </summary>
    Task<Workflow> TestAsync(WorkflowTestRequest workflowTestRequest, CancellationToken cancellationToken = default);
}
