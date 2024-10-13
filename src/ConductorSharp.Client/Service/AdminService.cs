using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public class AdminService(HttpClient client) : IAdminService
    {
        private readonly ConductorClient _client = new(client);

        public async Task<string> QueueRunningWorkflowsForSweepAsync(string workflowId, CancellationToken cancellationToken = default) =>
            await _client.RequeueSweepAsync(workflowId, cancellationToken);

        public async Task<string> VerifyAndRepairWorkflowConsistencyAsync(string workflowId, CancellationToken cancellationToken = default) =>
            await _client.VerifyAndRepairWorkflowConsistencyAsync(workflowId, cancellationToken);

        public async Task<ICollection<Generated.Task>> ListPendingTasksAsync(
            string taskType,
            int? start,
            int? count,
            CancellationToken cancellationToken = default
        ) => await _client.ViewAsync(taskType, start, count, cancellationToken);

        public async Task<IDictionary<string, object>> GetEventQueueMapAsync(bool? verbose, CancellationToken cancellationToken = default) =>
            await _client.GetEventQueuesAsync(verbose, cancellationToken);

        public async Task<IDictionary<string, object>> GetConfigMapAsync(CancellationToken cancellationToken = default) =>
            await _client.GetAllConfigAsync(cancellationToken);
    }
}
