using ConductorSharp.Client.Service;
using ConductorSharp.Proxy.Models;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Proxy.Services
{
    public class WorkflowPollingBackgroundService : IHostedService, IDisposable
    {
        private readonly IPolledWokflowRegistry _polledWokflowRegistry;
        private readonly IWorkflowService _workflowService;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private Task _runningTask;

        public WorkflowPollingBackgroundService(IPolledWokflowRegistry polledWokflowRegistry, IWorkflowService workflowService)
        {
            _polledWokflowRegistry = polledWokflowRegistry;
            _workflowService = workflowService;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _runningTask = Run();

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();

            await Task.WhenAny(_runningTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        private async Task Run()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var ids = _polledWokflowRegistry.GetPolledWorkflows();

                if (ids.Count == 0)
                {
                    continue;
                }

                var tasks = new List<Task<JObject>>();

                //var batchSize = 100;

                //int numberOfBatches = (int)Math.Ceiling((double)ids.Count() / batchSize);

                //for (int i = 0; i < numberOfBatches; i++)
                //{
                //    var currentIds = ids.Skip(i * batchSize).Take(batchSize).ToList();
                //    tasks.Add(_workflowService.GetWorkflowStatus(currentIds.First()));
                //}

                tasks.Add(_workflowService.GetWorkflowStatus(ids.First()));

                var objs = await Task.WhenAll(tasks);

                var results = objs.Select(a => a.ToObject<TaskPollResult>());

                foreach (var result in results)
                {
                    if (result.Status == "RUNNING")
                    {
                        _polledWokflowRegistry.StopPolling(result.WorkflowId);
                        await _polledWokflowRegistry.PublishUpdate();
                    }
                }
            }
        }
    }
}
