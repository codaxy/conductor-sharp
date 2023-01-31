using ConductorSharp.ApiEnabled.Models;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Client.Model.Request;
using ConductorSharp.Client.Model.Response;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

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

    [HttpGet("status")]
    public async Task<string> GetStatus(string workflowId)
    {
        var response = await workflowService.GetWorkflowStatus(workflowId, false);
        return response.ToString();
    }

    [HttpGet("get-workflows")]
    public async Task<ActionResult<WorkflowDefinition[]>> GetRegisteredWorkflows() => await metadataService.GetAllWorkflowDefinitions();

    [HttpGet("get-task-logs")]
    public async Task<ActionResult<GetTaskLogsResponse[]>> GetTaskLogs(string taskId) => await taskService.GetLogsForTask(taskId);

    [HttpGet("get-executions")]
    public async Task<ActionResult<WorkflowSearchResponse>> SearchWorkflows([FromQuery] WorkflowSearchRequest request) =>
        await workflowService.SearchWorkflows(request);

    [HttpPost("send-notification")]
    public async Task<ActionResult<string>> QueueWorkflow([FromBody] SendNotificationRequest request) =>
        await workflowService.QueueWorkflowStringResponse(
            NotificationWorfklowName,
            1,
            new JObject { new JProperty("task_to_execute", "CUSTOMER_get"), new JProperty("customer_id", request.CustomerId) }
        );
}
