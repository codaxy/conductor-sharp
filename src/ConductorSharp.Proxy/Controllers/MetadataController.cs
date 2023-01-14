using ConductorSharp.Client.Model.Common;
using ConductorSharp.Client.Service;
using Microsoft.AspNetCore.Mvc;

namespace ConductorSharp.Proxy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetadataController : ControllerBase
    {
        private readonly IMetadataService _metadataService;

        public MetadataController(IMetadataService metadataService)
        {
            _metadataService = metadataService;
        }

        [HttpGet("taskdefs")]
        public async Task<ActionResult<TaskDefinition[]>> GetAllTaskDefinitions()
        {
            return await _metadataService.GetAllTaskDefinitions();
        }

        [HttpPost("taskdefs")]
        public async Task CreateTaskDefinitions(List<TaskDefinition> definitions)
        {
            await _metadataService.CreateTaskDefinitions(definitions);
        }

        [HttpGet("taskdefs/{name}")]
        public async Task<TaskDefinition> GetTaskDefinition([FromRoute] string name)
        {
            return await _metadataService.GetTaskDefinition(name);
        }

        [HttpDelete("taskdefs/{name}")]
        public async Task DeleteTaskDefinition(string name)
        {
            await _metadataService.DeleteEventHandlerDefinition(name);
        }

        [HttpPut("taskdefs/{name}")]
        public async Task UpdateTaskDefinition(TaskDefinition definition)
        {
            await _metadataService.UpdateTaskDefinition(definition);
        }

        [HttpGet("workflow/{name}")]
        public async Task<WorkflowDefinition> GetWorkflowDefinition([FromRoute] string name, [FromQuery] int version)
        {
            return await _metadataService.GetWorkflowDefinition(name, version);
        }

        //[HttpPut("workflow")]
        //public async Task UpdateWorkflowDefinition(WorkflowDefinition workflowDefinition)
        //{
        //    await _metadataService.UpdateWorkflowDefinition(workflowDefinition);
        //}

        [HttpDelete("workflow/{name}")]
        public async Task DeleteWorkflowDefinition([FromRoute] string name, [FromQuery] int version)
        {
            await _metadataService.DeleteWorkflowDefinition(name, version);
        }

        [HttpGet("workflow")]
        public async Task<WorkflowDefinition[]> GetAllWorkflowDefinitions()
        {
            return await _metadataService.GetAllWorkflowDefinitions();
        }

        //public async Task<EventHandlerDefinition[]> GetAllEventHandlerDefinitions()
        //{
        //    return await _metadataService.GetAllEventHandlerDefinitions();
        //}

        //public async Task UpdateEventHandlerDefinition(EventHandlerDefinition definition)
        //{
        //    await _metadataService.UpdateEventHandlerDefinition(definition);
        //}

        //public async Task DeleteEventHandlerDefinition(string name)
        //{
        //    await _metadataService.DeleteEventHandlerDefinition(name);
        //}

        //public async Task CreateEventHandlerDefinition(EventHandlerDefinition definition)
        //{
        //    await _metadataService.CreateEventHandlerDefinition(definition);
        //}

        //public async Task<EventHandlerDefinition> GetEventHandlerDefinition(string name)
        //{
        //    return await _metadataService.GetEventHandlerDefinition(name);
        //}

        [HttpPut("workflow")]
        public async Task CreateWorkflowDefinitions(List<WorkflowDefinition> definition)
        {
            await _metadataService.CreateWorkflowDefinitions(definition);
        }
    }
}
