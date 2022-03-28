using AutoMapper;
using ConductorSharp.ApiEnabled.Handlers;
using ConductorSharp.ApiEnabled.Models;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Client.Service;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using  TaskDef = ConductorSharp.Client.Model.Common.TaskDefinition;

namespace ConductorSharp.ApiEnabled.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkflowController : ControllerBase
{
    private readonly IMetadataService metadataService;
    private readonly IWorkflowService workflowService;

    private const string NotificationWorfklowName = "NOTIFICATION_send_to_customer";

    public WorkflowController(IMetadataService metadataService, IWorkflowService workflowService)
    {
        this.metadataService = metadataService;
        this.workflowService = workflowService;
    }


    [HttpGet("get-workflows")]
    public async Task<ActionResult<WorkflowDefinition[]>> GetRegisteredWorkflows() => await metadataService.GetAllWorkflowDefinitions();

    [HttpPost("send-notification")]
    public async Task<ActionResult<string>> QueueWorkflow([FromBody] SendNotificationRequest request) => await workflowService.QueueWorkflowStringResponse(NotificationWorfklowName, 1, new JObject
    {
        new JProperty("customer_id", request.CustomerId)
    });


}
