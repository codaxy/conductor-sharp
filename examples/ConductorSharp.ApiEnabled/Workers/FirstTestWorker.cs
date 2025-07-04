using ConductorSharp.Engine;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.ApiEnabled.Workers
{
    public class FirstTestWorker : ITaskInput<FirstTestWorker.Response>
    {
        public string Input { get; set; }

        public class Response
        {
            public string Output { get; set; }
        }

        public class Worker : Worker<FirstTestWorker, Response>
        {
            private readonly ILogger<Worker> _logger;

            public Worker(ILogger<Worker> logger)
            {
                _logger = logger;
            }

            public override Task<Response> Handle(FirstTestWorker test, WorkerExecutionContext context, CancellationToken cancellationToken)
            {
                _logger.LogInformation("First test worker");

                return Task.FromResult<Response>(new() { Output = test.Input });
            }
        }
    }
}
