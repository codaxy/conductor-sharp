using ConductorSharp.Engine.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConductorSharp.ApiEnabled.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CounterController : ControllerBase
    {
        private readonly ITaskExecutionCounterService _taskExecutionCounterService;

        public CounterController(ITaskExecutionCounterService taskExecutionCounterService)
        {
            _taskExecutionCounterService = taskExecutionCounterService;
        }

        [HttpGet("completed")]
        public ActionResult<Dictionary<string, int>> GetCompleted()
        {
            return Ok(_taskExecutionCounterService.GetCompletedCount());
        }

        [HttpGet("failed")]
        public ActionResult<Dictionary<string, int>> GetFailed()
        {
            return Ok(_taskExecutionCounterService.GetFailedCount());
        }

        [HttpGet("running")]
        public ActionResult<List<RunningTask>> GetRunning()
        {
            return Ok(_taskExecutionCounterService.GetRunning());
        }
    }
}
