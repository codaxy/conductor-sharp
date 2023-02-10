using Autofac;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Tests.Samples.Workflows;
using ConductorSharp.Engine.Util.Builders;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class WorkflowItemRegistryTests
    {
        [Fact]
        public void RegisteredItemsResolve()
        {
            var builder = new ContainerBuilder();

            builder
                .AddConductorSharp(baseUrl: "empty", apiPath: "empty")
                .AddExecutionManager(
                    maxConcurrentWorkers: 1,
                    sleepInterval: 1,
                    longPollInterval: 1,
                    handlersAssembly: typeof(WorkflowItemRegistryTests).Assembly
                );

            builder.RegisterWorkflow<WorkflowItemTestWorkflow>();
            var container = builder.Build();

            // Items are registered when building the workflow definition, so we have to resolve them first
            _ = container.Resolve<IEnumerable<WorkflowDefinition>>().ToList();

            var registry = container.Resolve<WorkflowBuildItemRegistry>();

            registry.TryGet<WorkflowItemTestWorkflow>(out var items);

            Assert.Equal(WorkflowItemTestWorkflow.TestValue, items[WorkflowItemTestWorkflow.TestKey]);
        }

        [Fact]
        public void GetAllCorrectlyResolves()
        {
            var builder = new ContainerBuilder();

            builder
                .AddConductorSharp(baseUrl: "empty", apiPath: "empty")
                .AddExecutionManager(
                    maxConcurrentWorkers: 1,
                    sleepInterval: 1,
                    longPollInterval: 1,
                    handlersAssembly: typeof(WorkflowItemRegistryTests).Assembly
                );

            builder.RegisterWorkflow<WorkflowItemTestWorkflow>();
            var container = builder.Build();

            // Items are registered when building the workflow definition, so we have to resolve them first
            _ = container.Resolve<IEnumerable<WorkflowDefinition>>().ToList();

            var registry = container.Resolve<WorkflowBuildItemRegistry>();

            registry.Register<WorkflowItemTestWorkflow>("definition", new TaskDefinition { CreatedBy = "test1test1" });

            registry.Register<WorkflowItemTestWorkflow>("definition2", new TaskDefinition { CreatedBy = "test1test2" });

            registry.Register<WorkflowItemTestWorkflow>("definition3", new WorkflowDefinition { CreatedBy = "test1workflow1" });

            registry.TryGet<WorkflowItemTestWorkflow>(out var items);

            var taskDefs = registry.GetAll<TaskDefinition>();
            var workflowDefs = registry.GetAll<WorkflowDefinition>();
            var ints = registry.GetAll<int>();

            Assert.Equal(2, taskDefs.Count);
            Assert.Single(workflowDefs);
            Assert.Empty(ints);
        }
    }
}
