using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Tests.Samples.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class WorkflowItemRegistryTests
    {
        [Fact]
        public void RegisteredItemsResolve()
        {
            var builder = new ServiceCollection();

            builder
                .AddConductorSharp(baseUrl: "http://empty/empty")
                .AddExecutionManager(maxConcurrentWorkers: 1, sleepInterval: 1, longPollInterval: 1, null);

            builder.RegisterWorkflow<WorkflowItemTestWorkflow>();
            var container = builder.BuildServiceProvider();

            // Items are registered when building the workflow definition, so we have to resolve them first
            _ = container.GetRequiredService<IEnumerable<WorkflowDef>>().ToList();

            var registry = container.GetRequiredService<WorkflowBuildItemRegistry>();

            registry.TryGet<WorkflowItemTestWorkflow>(out var items);

            Assert.Equal(WorkflowItemTestWorkflow.TestValue, items[WorkflowItemTestWorkflow.TestKey]);
        }

        [Fact]
        public void GetAllCorrectlyResolves()
        {
            var builder = new ServiceCollection();

            builder
                .AddConductorSharp(baseUrl: "http://empty/empty")
                .AddExecutionManager(maxConcurrentWorkers: 1, sleepInterval: 1, longPollInterval: 1, null);

            builder.RegisterWorkflow<WorkflowItemTestWorkflow>();
            var container = builder.BuildServiceProvider();

            // Items are registered when building the workflow definition, so we have to resolve them first
            _ = container.GetRequiredService<IEnumerable<WorkflowDef>>().ToList();

            var registry = container.GetRequiredService<WorkflowBuildItemRegistry>();

            registry.Register<WorkflowItemTestWorkflow>("definition", new TaskDef { CreatedBy = "test1test1" });

            registry.Register<WorkflowItemTestWorkflow>("definition2", new TaskDef { CreatedBy = "test1test2" });

            registry.Register<WorkflowItemTestWorkflow>("definition3", new WorkflowDef { CreatedBy = "test1workflow1" });

            registry.TryGet<WorkflowItemTestWorkflow>(out _);

            var taskDefs = registry.GetAll<TaskDef>();
            var workflowDefs = registry.GetAll<WorkflowDef>();
            var ints = registry.GetAll<int>();

            Assert.Equal(2, taskDefs.Count);
            Assert.Single(workflowDefs);
            Assert.Empty(ints);
        }
    }
}
