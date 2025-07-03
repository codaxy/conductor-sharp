using System.Diagnostics;
using ConductorSharp.ApiEnabled.Workflows;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Util;
using Microsoft.Extensions.DependencyInjection;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Engine.IntegrationTests
{
    public class WorfklowExecutionTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public WorfklowExecutionTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task StartWorkflowAsync()
        {
            var workflowService = _factory.Services.GetRequiredService<IWorkflowService>();
            var workflowId = await workflowService.StartAsync(new() { Name = NamingUtil.NameOf<TestWorkflow.Workflow>(), Version = 1 });

            var wf = await WaitForWorkflowTermination(workflowId);
            Assert.Equal(WorkflowStatus.COMPLETED, wf.Status);
        }

        private async Task<Workflow> WaitForWorkflowTermination(string workflowId)
        {
            var workflowService = _factory.Services.GetRequiredService<IWorkflowService>();
            var timeout = TimeSpan.FromMinutes(5);
            var sw = Stopwatch.StartNew();
            Workflow wf = null!;

            while (sw.Elapsed < timeout)
            {
                wf = await workflowService.GetExecutionStatusAsync(workflowId);

                if (wf.Status != WorkflowStatus.RUNNING)
                    break;

                await Task.Delay(1000);
            }

            return wf;
        }
    }
}
