using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public class AdminService(ConductorClient client) : IAdminService
    {
        public async Task<string> QueueWorkflowsForSweep(string workflowId, CancellationToken cancellationToken = default)
            => await client.RequeueSweepAsync(workflowId, cancellationToken);

        public async Task<string> VerifyAndRepairWorkflowConsistency(string workflowId,
            CancellationToken cancellationToken = default)
            => await client.VerifyAndRepairWorkflowConsistencyAsync(workflowId, cancellationToken);

        public async Task<ICollection<ConductorSharp.Client.Generated.Task>> GetPendingTasks(string taskType, int? start, int? count,
            CancellationToken cancellationToken = default)
            => await client.ViewAsync(taskType, start, count, cancellationToken);

        public async Task<IDictionary<string, object>> GetEventQueuesAsync(bool? verbose,
            CancellationToken cancellationToken = default)
            => await client.GetEventQueuesAsync(verbose, cancellationToken);

        public async Task<IDictionary<string, object>> GetAllConfigAsync(CancellationToken cancellationToken = default)
            => await client.GetAllConfigAsync(cancellationToken);
    }
}
