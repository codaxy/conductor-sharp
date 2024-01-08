using ConductorSharp.ApiEnabled.Models;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using Microsoft.AspNetCore.Mvc;

namespace ConductorSharp.ApiEnabled.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkflowController : ControllerBase
{
    private readonly IMetadataService _metadataService;
    private readonly IWorkflowService _workflowService;
    private readonly ITaskService _taskService;
    private const string NotificationWorfklowName = "NOTIFICATION_send_to_customer";

    public WorkflowController(IMetadataService metadataService, IWorkflowService workflowService, ITaskService taskService)
    {
        _metadataService = metadataService;
        _workflowService = workflowService;
        _taskService = taskService;
    }

    [HttpGet("get-workflows")]
    public async Task<ICollection<WorkflowDef>> GetRegisteredWorkflows() => await _metadataService.GetAllWorkflowsAsync();

    [HttpGet("get-task-logs")]
    public async Task<ICollection<TaskExecLog>> GetTaskLogs(string taskId) => await _taskService.GetLogsAsync(taskId);

    [HttpGet("get-executions")]
    public async Task<SearchResultWorkflow> SearchWorkflows([FromQuery] int? start = null, [FromQuery] int? size = null) =>
        await _workflowService.SearchV2Async(start, size);

    [HttpGet("get-status/{workflowId}")]
    public async Task<Workflow> GetStatus([FromRoute] string workflowId, [FromQuery] bool includeTasks) =>
        await _workflowService.GetExecutionStatusAsync(workflowId, includeTasks);

    [HttpPost("send-notification")]
    public async Task<ActionResult<string>> QueueWorkflow([FromBody] SendNotificationRequest request) =>
        await _workflowService.StartAsync(
            new StartWorkflowRequest
            {
                Name = NotificationWorfklowName,
                Version = 1,
                Input = new Dictionary<string, object> { { "task_to_execute", "CUSTOMER_get" }, { "customer_id", request.CustomerId } }
            }
        );
}
