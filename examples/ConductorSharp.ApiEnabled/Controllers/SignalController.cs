using ConductorSharp.ApiEnabled.Models;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Patterns.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConductorSharp.ApiEnabled.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SignalController : ControllerBase
{
    private readonly ISignalService _signalService;
    private readonly IWorkflowService _workflowService;
    private const string SignalTestWorkflowName = "SIGNAL_test_workflow";

    public SignalController(ISignalService signalService, IWorkflowService workflowService)
    {
        _signalService = signalService;
        _workflowService = workflowService;
    }

    [HttpPost("start-test-workflow")]
    public async Task<ActionResult<string>> StartSignalTestWorkflow([FromBody] StartSignalTestWorkflowRequest request)
    {
        var workflowId = await _workflowService.StartAsync(
            new StartWorkflowRequest
            {
                Name = SignalTestWorkflowName,
                Version = 1,
                Input = new Dictionary<string, object> { { "signal_key", request.SignalKey }, { "message", request.Message } }
            }
        );

        return Ok(new { WorkflowId = workflowId, SignalKey = request.SignalKey });
    }

    [HttpPost("send")]
    public async Task<ActionResult> SendSignal([FromBody] SendSignalRequest request)
    {
        await _signalService.SendSignalAsync(request.SignalKey, TaskResultStatus.COMPLETED, request.OutputData);

        return Ok(new { Message = $"Signal '{request.SignalKey}' sent successfully" });
    }

    [HttpPost("send-failure")]
    public async Task<ActionResult> SendFailureSignal([FromBody] SendSignalRequest request)
    {
        await _signalService.SendSignalAsync(request.SignalKey, TaskResultStatus.FAILED, request.OutputData);

        return Ok(new { Message = $"Failure signal '{request.SignalKey}' sent successfully" });
    }
}
