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
        private Task? _runningTask;

        public WorkflowPollingBackgroundService(IPolledWokflowRegistry polledWokflowRegistry, IWorkflowService workflowService)
        {
            _polledWokflowRegistry = polledWokflowRegistry;
            _workflowService = workflowService;
        }

        public void Dispose() { }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _runningTask = Run(_cancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();

            if (_runningTask != null)
                await Task.WhenAny(_runningTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        private async Task Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var ids = _polledWokflowRegistry.GetPolledWorkflows();

                    if (ids.Count != 0)
                    {
                        var tasks = new List<Task<JObject>>();

                        foreach (var wf in ids)
                        {
                            tasks.Add(_workflowService.GetWorkflowStatus(wf));
                        }

                        var objs = await Task.WhenAll(tasks);

                        var results = objs.Where(a => a != null).Select(a => a.ToObject<TaskPollResult>());

                        foreach (var result in results)
                        {
                            await _polledWokflowRegistry.PublishUpdate(result!);
                        }
                    }

                    await Task.Delay(1000, cancellationToken);
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc);
                }
            }
        }
    }
}
