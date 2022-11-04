using ConductorSharp.ApiEnabled.Models;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Client.Model.Request;
using ConductorSharp.Client.Model.Response;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Interface;
using ConductorSharp.ScaffoldedDefinitions.Workflows;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.ApiEnabled.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkflowController : ControllerBase
{
    private readonly IMetadataService _metadataService;
    private readonly IWorkflowService _workflowService;
    private readonly ITaskService _taskService;
    private readonly ITypedWorkflowService _typedWorkflowService;
    private const string NotificationWorfklowName = "NOTIFICATION_send_to_customer";

    public WorkflowController(
        IMetadataService metadataService,
        IWorkflowService workflowService,
        ITaskService taskService,
        ITypedWorkflowService typedWorkflowService
    )
    {
        _metadataService = metadataService;
        _workflowService = workflowService;
        _taskService = taskService;
        _typedWorkflowService = typedWorkflowService;
    }

    [HttpGet("get-workflows")]
    public async Task<ActionResult<WorkflowDefinition[]>> GetRegisteredWorkflows() => await _metadataService.GetAllWorkflowDefinitions();

    [HttpGet("get-task-logs")]
    public async Task<ActionResult<GetTaskLogsResponse[]>> GetTaskLogs(string taskId) => await _taskService.GetLogsForTask(taskId);

    [HttpGet("get-executions")]
    public async Task<ActionResult<WorkflowSearchResponse>> SearchWorkflows([FromQuery] WorkflowSearchRequest request) =>
        await _workflowService.SearchWorkflows(request);

    [HttpPost("send-notification")]
    public async Task<ActionResult<string>> QueueWorkflow([FromBody] SendNotificationRequest request) =>
        //await _workflowService.QueueWorkflowStringResponse(
        //    NotificationWorfklowName,
        //    1,
        //    new JObject { new JProperty("task_to_execute", "CUSTOMER_get"), new JProperty("customer_id", request.CustomerId) }
        //);

        await _typedWorkflowService.StartWorkflow<NotificationSendToCustomerV1>(new NotificationSendToCustomerV1Input { });
}
