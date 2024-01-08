using ConductorSharp.Client.Generated;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Client.Service
{
    public class QueueAdminService(ConductorClient client) : IQueueAdminService
    {
        public async Task MarkWaitTaskAsAsync(string workflowid, string taskRefName, Status status,
            IDictionary<string, object> output, CancellationToken cancellationToken = default)
             => await client.Update_1Async(workflowid, taskRefName, status, output, cancellationToken);

        public async Task MarkWaitTaskByTaskIdAsAsync(string workflowId, string taskId, Status2 status,
            IDictionary<string, object> output, CancellationToken cancellationToken = default)
            => await client.UpdateByTaskIdAsync(workflowId, taskId, status, output, cancellationToken);

        public async Task<IDictionary<string, long>> GetQueueLengthAsync(CancellationToken cancellationToken = default)
            => await client.Size_1Async(cancellationToken);

        public async Task<IDictionary<string, string>> GetQueueNames(CancellationToken cancellationToken = default)
            => await client.NamesAsync(cancellationToken);
    }
}
