using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public class TaskService(ConductorClient client) : ITaskService
    {
        private readonly ConductorClient _client = client;

        public async Task<string> UpdateAsync(TaskResult updateRequest, CancellationToken cancellationToken = default)
            => await _client.UpdateTaskAsync(updateRequest, cancellationToken);

        public async Task<ICollection<TaskExecLog>> GetLogsAsync(string taskId, CancellationToken cancellationToken = default)
            => await _client.GetTaskLogsAsync(taskId, cancellationToken);

        public async Task LogAsync(string taskId, string message, CancellationToken cancellationToken = default)
            => await _client.LogAsync(taskId, message, cancellationToken);

        public async Task<string> RequeuePendingAsync(string taskType, CancellationToken cancellationToken = default)
          => await _client.RequeuePendingTaskAsync(taskType, cancellationToken);

        public async Task<ConductorSharp.Client.Generated.Task> GetAsync(string taskId, CancellationToken cancellationToken = default)
          => await _client.GetTaskAsync(taskId, cancellationToken);

        public async Task<SearchResultTaskSummary> SearchAsync(int? start = null, int? size = null, string? sort = null, string? freeText = null, string? query = null, CancellationToken cancellationToken = default)
            => await _client.Search_1Async(start, size, sort, freeText, query, cancellationToken);

        public async Task<SearchResultTask> SearchV2Async(int? start = null, int? size = null, string? sort = null, string? freeText = null, string? query = null, CancellationToken cancellationToken = default)
            => await _client.SearchV2_1Async(start, size, sort, freeText, query, cancellationToken);

        public async Task<int> GetQueueAsync(string taskType, string? domain = null, string? isolationGroupId = null, string? executionNamespace = null, CancellationToken cancellationToken = default)
            => await _client.TaskDepthAsync(taskType, domain, isolationGroupId, executionNamespace, cancellationToken);

        public async Task<ICollection<PollData>> GetPollDataAsync(string taskType, CancellationToken cancellationToken = default)
            => await _client.GetPollDataAsync(taskType, cancellationToken);

        public async Task<ICollection<PollData>> ListPollDataAsync(CancellationToken cancellationToken = default)
            => await _client.GetAllPollDataAsync(cancellationToken);

        public async Task<IDictionary<string, long>> ListQueuesAsync(CancellationToken cancellationToken = default)
            => await _client.AllAsync(cancellationToken);

        public async Task<IDictionary<string, IDictionary<string, IDictionary<string, long>>>> ListQueuesVerboseAsync(CancellationToken cancellationToken = default)
          => await _client.AllVerboseAsync(cancellationToken);

        public async Task<Generated.Task> PollAsync(string taskType, string? workerId, string? domain, CancellationToken cancellationToken = default)
         => await _client.PollAsync(taskType, workerId, domain, cancellationToken);

        public async Task<ICollection<Generated.Task>> BatchPollAsync(string taskType, string? workerId, string? domain, int? count = null, int? timeout = null, CancellationToken cancellationToken = default)
            => await _client.BatchPollAsync(taskType, workerId, domain, count, timeout, cancellationToken);

        public async Task<ExternalStorageLocation> GetExternalStorageLocationAsync(string path, string operation, string payloadType, CancellationToken cancellationToken = default)
          => await _client.GetExternalStorageLocation_1Async(path, operation, payloadType, cancellationToken);

    }
}
