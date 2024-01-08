using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service;

public interface IMetadataService
{
    /// <summary>
    /// Create a new workflow definition
    /// </summary>
    Task AddWorkflowAsync(WorkflowDef workflowDef, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a task definition
    /// </summary>
    Task DeleteTaskAsync(string taskType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes workflow definition. It does not remove workflows associated with the definition.
    /// </summary>
    Task DeleteWorkflowAsync(string name, int version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all workflow definition along with blueprint
    /// </summary>
    Task<ICollection<WorkflowDef>> ListWorkflowsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns only the latest version of all workflow definitions
    /// </summary>
    Task<ICollection<WorkflowDef>> GetAllWorkflowsWithLatestVersionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the task definition
    /// </summary>
    Task<TaskDef> GetTaskAsync(string taskType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all task definition
    /// </summary>
    Task<ICollection<TaskDef>> ListTasksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves workflow definition along with blueprint
    /// </summary>
    Task<WorkflowDef> GetWorkflowAsync(string name, int? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns workflow names and versions only (no definition bodies)
    /// </summary>
    Task<IDictionary<string, object>> GetWorkflowNamesAndVersionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing task
    /// </summary>
    Task AddTaskAsync(TaskDef taskDef, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new task definition(s)
    /// </summary>
    Task AddTasksAsync(IEnumerable<TaskDef> taskDefs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create or update workflow definition
    /// </summary>
    Task<BulkResponse> UpdateWorkflowsAsync(IEnumerable<WorkflowDef> workflows, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a new workflow definition
    /// </summary>
    Task ValidateWorkflowAsync(WorkflowDef workflowDef, CancellationToken cancellationToken = default);
}
