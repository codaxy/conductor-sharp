using ConductorSharp.Client.Model.Request;
using ConductorSharp.Client.Model.Response;
using ConductorSharp.Client.Service;
using ConductorSharp.Proxy.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Proxy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ISingleFetchService _lockAndPoller;
        private static bool shouldCache = false;

        public TasksController(ITaskService taskService, ISingleFetchService lockAndPoller)
        {
            _taskService = taskService;
            _lockAndPoller = lockAndPoller;
        }

        //tasks/externalstoragelocation?path={0}&operation=READ&payloadType=TASK_INPUT
        [HttpGet("externalstoragelocation")]
        public async Task<ExternalStorageResponse> FetchExternalStorageLocation([FromQuery] ExternalStorageRequest request)
        {
            return await _taskService.FetchExternalStorageLocation(request.Path);
        }

        //external/postgres/{0}
        [HttpGet("external/postgres/{uri}")]
        public async Task<JObject> FetchExternalStorage([FromRoute] string uri)
        {
            return await _taskService.FetchExternalStorage(uri);
        }

        //used
        [HttpGet("queue/all")]
        public async Task<IDictionary<string, int>> GetAllQueues()
        {
            if (shouldCache)
            {
                return await _lockAndPoller.Fetch(async () => await _taskService.GetAllQueues());
            }
            else
            {
                return await _taskService.GetAllQueues();
            }
        }

        //used
        [HttpGet("queue/caching")]
        public async Task<object> GetAllQueuesCaching()
        {
            return Task.FromResult(
                new { Ratio = _lockAndPoller.GetCacheRatio(), Cached = _lockAndPoller.GetCachedCount(), Polled = _lockAndPoller.GetPolledCount() }
            );
        }

        [HttpGet("queue/toggle")]
        public async Task<object> ToggleCache()
        {
            shouldCache = !shouldCache;
            return Task.FromResult(new { ShouldCache = shouldCache });
        }

        //used
        [HttpPost]
        public async Task UpdateTask(UpdateTaskRequest updateTaskRequest)
        {
            if (updateTaskRequest.Status == "COMPLETED")
            {
                await _taskService.UpdateTaskCompleted(updateTaskRequest.OutputData, updateTaskRequest.TaskId, updateTaskRequest.WorkflowInstanceId);
            }
            else
            {
                await _taskService.UpdateTaskFailed(
                    updateTaskRequest.TaskId,
                    updateTaskRequest.WorkflowInstanceId,
                    updateTaskRequest.ReasonForIncompletion
                );
            }
        }

        //tasks/poll/{0}?workerId={1}
        [HttpGet("poll/{name}")]
        public async Task<PollTaskResponse> PollTasks([FromRoute] string name, [FromQuery] string workerId)
        {
            return await _taskService.PollTasks(name, workerId);
        }
    }

    public class ExternalStorageRequest
    {
        public string Path { get; set; }
        public string Operation { get; set; }
        public string PayloadType { get; set; }
    }
}
