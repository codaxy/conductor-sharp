using ConductorSharp.ApiEnabled.Models;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using Microsoft.AspNetCore.Mvc;

namespace ConductorSharp.ApiEnabled.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkflowController : ControllerBase
{
    private readonly IMetadataService metadataService;
    private readonly IWorkflowService workflowService;
    private readonly ITaskService taskService;
    private const string NotificationWorfklowName = "NOTIFICATION_send_to_customer";

    public WorkflowController(IMetadataService metadataService, IWorkflowService workflowService, ITaskService taskService)
    {
        this.metadataService = metadataService;
        this.workflowService = workflowService;
        this.taskService = taskService;
    }

    [HttpGet("get-workflows")]
    public async Task<ICollection<WorkflowDef>> GetRegisteredWorkflows() => await metadataService.GetAllWorkflowsAsync();

    [HttpGet("get-task-logs")]
    public async Task<ICollection<TaskExecLog>> GetTaskLogs(string taskId) => await taskService.GetLogsAsync(taskId);

    [HttpGet("get-executions")]
    public async Task<SearchResultWorkflow> SearchWorkflows([FromQuery] int? start = null, [FromQuery] int? size = null) =>
        await workflowService.SearchV2Async(start, size);

    [HttpGet("get-status/{workflowId}")]
    public async Task<Workflow> GetStatus([FromRoute] string workflowId, [FromQuery] bool includeTasks) =>
        await workflowService.GetExecutionStatusAsync(workflowId, includeTasks);

    [HttpPost("send-notification")]
    public async Task<ActionResult<string>> QueueWorkflow([FromBody] SendNotificationRequest request) =>
        await workflowService.StartAsync(
            new StartWorkflowRequest
            {
                Name = NotificationWorfklowName,
                Version = 1,
                Input = new Dictionary<string, object> { { "task_to_execute", "CUSTOMER_get" }, { "customer_id", request.CustomerId } }
            }
        );
}
