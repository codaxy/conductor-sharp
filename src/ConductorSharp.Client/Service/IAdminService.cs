using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public interface IAdminService
    {
        /// <summary>
        /// Get all the configuration parameters
        /// </summary>
        Task<IDictionary<string, object>> GetConfigMapAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get registered queues
        /// </summary>
        Task<IDictionary<string, object>> GetEventQueueMapAsync(bool? verbose, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the list of pending tasks for a given task type
        /// </summary>
        Task<ICollection<Generated.Task>> ListPendingTasksAsync(
            string taskType,
            int? start,
            int? count,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Queue up all the running workflows for sweep
        /// </summary>
        Task<string> QueueRunningWorkflowsForSweepAsync(string workflowId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify and repair workflow consistency
        /// </summary>
        Task<string> VerifyAndRepairWorkflowConsistencyAsync(string workflowId, CancellationToken cancellationToken = default);
    }
}
