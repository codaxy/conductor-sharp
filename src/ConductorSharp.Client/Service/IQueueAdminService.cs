using ConductorSharp.Client.Generated;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public interface IQueueAdminService
    {
        /// <summary>
        /// Get the queue length
        /// </summary>
        Task<IDictionary<string, long>> GetQueueLengthAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Queue Names
        /// </summary>
        Task<IDictionary<string, string>> GetQueueNames(CancellationToken cancellationToken = default);

        /// <summary>
        /// Publish a message in queue to mark a wait task as completed.
        /// </summary>
        Task MarkWaitTaskAsAsync(
            string workflowid,
            string taskRefName,
            Status status,
            IDictionary<string, object> output,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Publish a message in queue to mark a wait task (by taskId) as completed.
        /// </summary>
        Task MarkWaitTaskByTaskIdAsAsync(
            string workflowId,
            string taskId,
            Status2 status,
            IDictionary<string, object> output,
            CancellationToken cancellationToken = default
        );
    }
}
