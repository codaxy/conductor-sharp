using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client.Generated;

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
        Task<IDictionary<string, string>> GetQueueNamesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Publish a message in queue to mark a wait task as completed.
        /// </summary>
        Task MarkWaitTaskCompletedAsync(
            string workflowId,
            string taskRefName,
            Status status,
            IDictionary<string, object> output,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Publish a message in queue to mark a wait task (by taskId) as completed.
        /// </summary>
        Task MarkWaitTaskCompletedAsync(
            string workflowId,
            string taskId,
            Status2 status,
            IDictionary<string, object> output,
            CancellationToken cancellationToken = default
        );
    }
}
