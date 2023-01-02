using ConductorSharp.Client.Service;
using ConductorSharp.Proxy.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Proxy.Controllers
{
    public class QueueWfRequest
    {
        public JObject Input { get; set; }

        public string Name { get; set; }

        public int Version { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;
        private readonly IPolledWokflowRegistry _polledWorkflowRegistry;

        public WorkflowController(IWorkflowService workflowService, IPolledWokflowRegistry polledWorkflowRegistry)
        {
            _workflowService = workflowService;
            _polledWorkflowRegistry = polledWorkflowRegistry;
        }

        [HttpPost]
        public async Task<string> StartWorkflow(QueueWfRequest request)
        {
            var workflowId = await _workflowService.QueueWorkflowStringResponse(request.Name, request.Version, request.Input);

            _polledWorkflowRegistry.Poll(workflowId);

            return workflowId;
        }
    }
}
