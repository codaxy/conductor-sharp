using ConductorSharp.Client.Model.Response;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public interface ITaskService
    {
        Task<PollTaskResponse[]> PollBatch(string name, string workerId, int count, int timeout);
        Task<PollTaskResponse[]> PollBatch(string name, string workerId, int count, int timeout, string domain);
        Task<ExternalStorageResponse> FetchExternalStorageLocation(string name);
        Task<JObject> FetchExternalStorage(string uri);
        Task<JObject> TaskSearch(int size, string query);
        Task<IDictionary<string, int>> GetQueue(string name);
        Task<IDictionary<string, int>> GetAllQueues();
        Task UpdateTaskCompleted(object outputData, string taskId, string workflowId);
        Task UpdateTaskFailed(string taskId, string workflowId, string reasonForIncompletion);
        Task UpdateTaskFailed(string taskId, string workflowId, string reasonForIncompletion, string logMessage);
        Task UpdateTaskFailed(object outputData, string taskId, string workflowId, string reasonForIncompletion, string logMessage);
        Task<PollTaskResponse> PollTasks(string name, string workerId);
        Task<PollTaskResponse> PollTasks(string name, string workerId, string domain);
        Task<GetTaskLogsResponse[]> GetLogsForTask(string taskId);
    }
}
