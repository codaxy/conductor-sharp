using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public interface ITaskService
    {
        /// <summary>
        /// Get task by Id
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Generated.Task> GetAsync(string taskId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get queue size for a task type.
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="domain"></param>
        /// <param name="isolationGroupId"></param>
        /// <param name="executionNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> GetQueueAsync(
            string taskType,
            string? domain = null,
            string? isolationGroupId = null,
            string? executionNamespace = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Get the external uri where the task payload is to be stored
        /// </summary>
        /// <param name="path"></param>
        /// <param name="operation"></param>
        /// <param name="payloadType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ExternalStorageLocation> GetExternalStorageLocationAsync(
            string path,
            string operation,
            string payloadType,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Get Task Execution Logs
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ICollection<TaskExecLog>> ListLogsAsync(string taskId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the last poll data for a given task type
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ICollection<PollData>> GetPollDataAsync(string taskType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the last poll data for all task types
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ICollection<PollData>> ListPollDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the details about each queue
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IDictionary<string, long>> ListQueuesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the details about each queue
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IDictionary<string, IDictionary<string, IDictionary<string, long>>>> ListQueuesVerboseAsync(
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Log Task Execution Details
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task LogAsync(string taskId, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Poll for a task of a certain type
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="workerId"></param>
        /// <param name="domain"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Generated.Task> PollAsync(string taskType, string? workerId, string? domain, CancellationToken cancellationToken = default);

        /// <summary>
        /// Batch poll for a task of a certain type
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="workerId"></param>
        /// <param name="domain"></param>
        /// <param name="count"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ICollection<Generated.Task>> BatchPollAsync(
            string taskType,
            string? workerId,
            string? domain,
            int? count = null,
            int? timeout = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Requeue pending tasks
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> RequeuePendingAsync(string taskType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Search for tasks based in payload and other parameters.
        /// Use sort options as sort=<field>:ASC|DESC e.g. sort=name&sort=workflowId:DESC. If order is not specified, defaults to ASC
        /// </summary>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="sort"></param>
        /// <param name="freeText"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SearchResultTaskSummary> SearchAsync(
            int? start = null,
            int? size = null,
            string? sort = null,
            string? freeText = null,
            string? query = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Search for tasks based in payload and other parameters
        /// Use sort options as sort=<field>:ASC|DESC e.g. sort=name&sort=workflowId:DESC. If order is not specified, defaults to ASC
        /// </summary>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="sort"></param>
        /// <param name="freeText"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SearchResultTask> SearchV2Async(
            int? start = null,
            int? size = null,
            string? sort = null,
            string? freeText = null,
            string? query = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Update a task
        /// </summary>
        /// <param name="updateRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> UpdateAsync(TaskResult updateRequest, CancellationToken cancellationToken = default);
    }
}
