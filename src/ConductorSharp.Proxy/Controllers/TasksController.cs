using ConductorSharp.Client.Model.Request;
using ConductorSharp.Client.Model.Response;
using ConductorSharp.Client.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Proxy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
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
            return await _taskService.GetAllQueues();
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
