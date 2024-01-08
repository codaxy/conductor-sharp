using ConductorSharp.Client.Generated;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public interface IMetadataService
    {
        /// <summary>
        /// Create a new workflow definition
        /// </summary>
        System.Threading.Tasks.Task CreateWorkflowAsync(WorkflowDef workflowDef, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove a task definition
        /// </summary>
        System.Threading.Tasks.Task DeleteTaskDefAsync(string taskType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes workflow definition. It does not remove workflows associated with the definition.
        /// </summary>
        System.Threading.Tasks.Task DeleteWorkflowAsync(string name, int version, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all workflow definition along with blueprint
        /// </summary>
        Task<ICollection<WorkflowDef>> GetAllWorkflowsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns only the latest version of all workflow definitions
        /// </summary>
        Task<ICollection<WorkflowDef>> GetAllWorkflowsWithLatestVersionsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the task definition
        /// </summary>
        Task<TaskDef> GetTaskDefAsync(string taskType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all task definition
        /// </summary>
        Task<ICollection<TaskDef>> GetTaskDefsAsync(CancellationToken cancellationToken = default);

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
        System.Threading.Tasks.Task RegisterTaskAsync(TaskDef taskDef, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create new task definition(s)
        /// </summary>
        System.Threading.Tasks.Task RegisterTasksAsync(IEnumerable<TaskDef> taskDefs, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create or update workflow definition
        /// </summary>
        Task<BulkResponse> UpdateWorkflowsAsync(IEnumerable<WorkflowDef> workflows, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates a new workflow definition
        /// </summary>
        System.Threading.Tasks.Task ValidateWorkflowAsync(WorkflowDef workflowDef, CancellationToken cancellationToken = default);
    }
}
