using ConductorSharp.Client.Model.Response;
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
    [Route("api/[controller]")]
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

        [HttpGet("{workflowId}")]
        public async Task<JObject> GetWorkflowStatus([FromRoute] string workflowId, [FromQuery] bool includeTasks)
        {
            return await _workflowService.GetWorkflowStatus(workflowId, includeTasks);
        }

        //workflow/search?start={request.Start}&size={request.Size}&sort={sortWithDirection}&freeText={request.FreeText}&query={request.Query
        [HttpGet("search")]
        public async Task<WorkflowSearchResponse> SearchWorkflows([FromQuery] SearchReq request)
        {
            return await _workflowService.SearchWorkflows(
                new Client.Model.Request.WorkflowSearchRequest
                {
                    FreeText = request.FreeText,
                    Query = request.Query,
                    Size = request.Size,
                    Start = request.Start,
                    Sort = request.Sort
                }
            );
        }
    }

    public class SearchReq
    {
        public string? SortWithDirection { get; set; }
        public string? FreeText { get; set; }
        public string? Query { get; set; }
        public int? Start { get; set; }
        public int? Size { get; set; }
        public string? Sort { get; set; }
    }
}
