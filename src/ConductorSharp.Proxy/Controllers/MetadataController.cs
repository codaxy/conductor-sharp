using ConductorSharp.Client.Model.Common;
using ConductorSharp.Client.Service;
using Microsoft.AspNetCore.Mvc;

namespace ConductorSharp.Proxy.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MetadataController : ControllerBase
    {
        private readonly IMetadataService _metadataService;

        public MetadataController(IMetadataService metadataService)
        {
            _metadataService = metadataService;
        }

        [HttpGet("taskdefs")]
        public async Task<ActionResult<TaskDefinition[]>> Get()
        {
            return await _metadataService.GetAllTaskDefinitions();
        }
    }
}
