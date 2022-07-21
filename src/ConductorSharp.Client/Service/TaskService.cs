using ConductorSharp.Client.Model.Request;
using ConductorSharp.Client.Model.Response;
using ConductorSharp.Client.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static ConductorSharp.Client.Model.Request.UpdateTaskRequest;

namespace ConductorSharp.Client.Service
{
    public class TaskService : ITaskService
    {
        private readonly IConductorClient _client;

        public TaskService(IConductorClient client) => _client = client;

        public async Task<PollTaskResponse[]> PollBatch(string name, string workerId, int count, int timeout) =>
            (
                await _client.ExecuteRequestAsync<List<PollTaskResponse>>(ApiUrls.BatchPollTasks(name, workerId, count, timeout), HttpMethod.Get)
            ).ToArray();

        public async Task<PollTaskResponse[]> PollBatch(string name, string workerId, int count, int timeout, string domain) =>
            (
                await _client.ExecuteRequestAsync<List<PollTaskResponse>>(
                    ApiUrls.BatchPollTasks(name, workerId, count, timeout, domain),
                    HttpMethod.Get
                )
            ).ToArray();

        public async Task<ExternalStorageResponse> FetchExternalStorageLocation(string name) =>
            await _client.ExecuteRequestAsync<ExternalStorageResponse>(ApiUrls.FetchExternalStorageLocation(name), HttpMethod.Get);

        // TODO: Check if this is actually relative url
        public async Task<JObject> FetchExternalStorage(string filename) =>
            await _client.ExecuteRequestAsync<JObject>(ApiUrls.GetExternalStorage(filename), HttpMethod.Get);

        private async Task UpdateTask(UpdateTaskRequest data) => await _client.ExecuteRequestAsync(ApiUrls.UpdateTask(), HttpMethod.Post, data);

        public async Task<JObject> TaskSearch(int size, string query) =>
            await _client.ExecuteRequestAsync<JObject>(ApiUrls.SearchTask(size, query), HttpMethod.Get);

        public async Task<IDictionary<string, int>> GetQueue(string name) =>
            await _client.ExecuteRequestAsync<IDictionary<string, int>>(ApiUrls.GetTaskQueue(name), HttpMethod.Get);

        public async Task<IDictionary<string, int>> GetAllQueues() =>
            await _client.ExecuteRequestAsync<IDictionary<string, int>>(ApiUrls.GetAllQueues(), HttpMethod.Get);

        public Task UpdateTaskCompleted(object outputData, string taskId, string workflowId) =>
            UpdateTask(
                new UpdateTaskRequest
                {
                    Status = "COMPLETED",
                    OutputData = JObject.FromObject(outputData, ConductorConstants.IoJsonSerializer),
                    TaskId = taskId,
                    WorkflowInstanceId = workflowId
                }
            );

        public Task UpdateTaskFailed(string taskId, string workflowId, string reasonForIncompletion) =>
            UpdateTask(
                new UpdateTaskRequest
                {
                    Status = "FAILED",
                    TaskId = taskId,
                    WorkflowInstanceId = workflowId,
                    ReasonForIncompletion = reasonForIncompletion,
                    Logs = new List<LogData>
                    {
                        new LogData { Log = reasonForIncompletion, CreatedTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() }
                    }
                }
            );

        public Task UpdateTaskFailed(string taskId, string workflowId, string reasonForIncompletion, string logMessage) =>
            UpdateTask(
                new UpdateTaskRequest
                {
                    Status = "FAILED",
                    TaskId = taskId,
                    WorkflowInstanceId = workflowId,
                    ReasonForIncompletion = reasonForIncompletion,
                    Logs = new List<LogData>
                    {
                        new LogData { Log = reasonForIncompletion, CreatedTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() },
                        new LogData { Log = logMessage, CreatedTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() }
                    }
                }
            );

        public async Task<PollTaskResponse> PollTasks(string name, string workerId) =>
            await _client.ExecuteRequestAsync<PollTaskResponse>(ApiUrls.PollTasks(name, workerId), HttpMethod.Get);

        public async Task<PollTaskResponse> PollTasks(string name, string workerId, string domain) =>
            await _client.ExecuteRequestAsync<PollTaskResponse>(ApiUrls.PollTasks(name, workerId, domain), HttpMethod.Get);

        public Task<GetTaskLogsResponse[]> GetLogsForTask(string taskId) =>
            _client.ExecuteRequestAsync<GetTaskLogsResponse[]>(ApiUrls.GetLogsForTask(taskId), HttpMethod.Get);
    }
}
