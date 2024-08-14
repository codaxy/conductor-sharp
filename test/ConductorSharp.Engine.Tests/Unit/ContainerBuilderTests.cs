using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Tests.Samples.Workflows;
using ConductorSharp.Engine.Tests.Util;
using ConductorSharp.Engine.Util.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class ContainerBuilderTests
    {
        [Fact]
        public void AppliesBuildConfigurationDefaults()
        {
            var builder = new ServiceCollection();

            builder
                .AddConductorSharp(baseUrl: "http://empty/empty")
                .SetBuildConfiguration(new BuildConfiguration { DefaultOwnerApp = "testApp", DefaultOwnerEmail = "owner@test.app" })
                .AddExecutionManager(
                    maxConcurrentWorkers: 1,
                    sleepInterval: 1,
                    longPollInterval: 1,
                    domain: null,
                    handlerAssemblies: typeof(ContainerBuilderTests).Assembly
                );

            builder.RegisterWorkflow<StringInterpolation>();
            var container = builder.BuildServiceProvider();

            var definitions = container.GetRequiredService<IEnumerable<WorkflowDef>>().ToList();

            Assert.True(definitions.All(a => a.OwnerApp == "testApp"));
        }

        [Fact]
        public void OverridesBuildConfigurationDefaults()
        {
            var builder = new ServiceCollection();
            var overrideValue = "override";
            builder
                .AddConductorSharp(baseUrl: "http://empty/empty")
                .SetBuildConfiguration(new BuildConfiguration { DefaultOwnerApp = "testApp", DefaultOwnerEmail = "owner@test.app", })
                .AddExecutionManager(
                    maxConcurrentWorkers: 1,
                    sleepInterval: 1,
                    longPollInterval: 1,
                    null,
                    handlerAssemblies: typeof(ContainerBuilderTests).Assembly
                );

            builder.RegisterWorkflow<StringInterpolation>(new BuildConfiguration { DefaultOwnerApp = overrideValue });
            var container = builder.BuildServiceProvider();

            var definitions = container.GetRequiredService<IEnumerable<WorkflowDef>>().ToList();

            Assert.True(definitions.All(a => a.OwnerApp == overrideValue));
        }

        [Fact]
        public void ResolveWorkflowDependencies()
        {
            var builder = new ServiceCollection();
            builder
                .AddConductorSharp(baseUrl: "http://empty/empty")
                .AddExecutionManager(
                    maxConcurrentWorkers: 1,
                    sleepInterval: 1,
                    longPollInterval: 1,
                    null,
                    handlerAssemblies: typeof(ContainerBuilderTests).Assembly
                );

            builder.AddTransient<IConfigurationService, ConfigurationService>();
            builder.RegisterWorkflow<WorkflowWithDependencies>();
            var container = builder.BuildServiceProvider();
            var definitions = container.GetRequiredService<IEnumerable<WorkflowDef>>();
            Assert.Contains(definitions, def => def.Name == NamingUtil.NameOf<WorkflowWithDependencies>());
        }

        [Fact]
        public void FailsToResolveWorkflowDependencies()
        {
            var builder = new ServiceCollection();
            builder
                .AddConductorSharp(baseUrl: "http://empty/empty")
                .AddExecutionManager(
                    maxConcurrentWorkers: 1,
                    sleepInterval: 1,
                    longPollInterval: 1,
                    null,
                    handlerAssemblies: typeof(ContainerBuilderTests).Assembly
                );

            builder.RegisterWorkflow<WorkflowWithDependencies>();
            var container = builder.BuildServiceProvider();
            Assert.Throws<InvalidOperationException>(() => container.GetRequiredService<IEnumerable<WorkflowDef>>());
        }

        [Fact]
        public void SuccesfullyResolveClients()
        {
            const string alternateClient = "Alternate";
            var builder = new ServiceCollection();
            builder.AddConductorSharp(baseUrl: "http://empty/empty").AddAlternateClient("http://alternate/alternate", alternateClient);

            var container = builder.BuildServiceProvider();

            var adminService = container.GetService<IAdminService>();
            var eventService = container.GetService<IEventService>();
            var externalPayloadService = container.GetService<IExternalPayloadService>();
            var queueAdminService = container.GetService<IQueueAdminService>();
            var workflowBulkService = container.GetService<IWorkflowBulkService>();
            var taskService = container.GetService<ITaskService>();
            var healthService = container.GetService<IHealthService>();
            var metadataService = container.GetService<IMetadataService>();
            var workflowService = container.GetService<IWorkflowService>();

            Assert.NotNull(adminService);
            Assert.NotNull(eventService);
            Assert.NotNull(externalPayloadService);
            Assert.NotNull(queueAdminService);
            Assert.NotNull(workflowBulkService);
            Assert.NotNull(taskService);
            Assert.NotNull(healthService);
            Assert.NotNull(metadataService);
            Assert.NotNull(workflowService);

            adminService = container.GetKeyedService<IAdminService>(alternateClient);
            eventService = container.GetKeyedService<IEventService>(alternateClient);
            externalPayloadService = container.GetKeyedService<IExternalPayloadService>(alternateClient);
            queueAdminService = container.GetKeyedService<IQueueAdminService>(alternateClient);
            workflowBulkService = container.GetKeyedService<IWorkflowBulkService>(alternateClient);
            taskService = container.GetKeyedService<ITaskService>(alternateClient);
            healthService = container.GetKeyedService<IHealthService>(alternateClient);
            metadataService = container.GetKeyedService<IMetadataService>(alternateClient);
            workflowService = container.GetKeyedService<IWorkflowService>(alternateClient);

            Assert.NotNull(adminService);
            Assert.NotNull(eventService);
            Assert.NotNull(externalPayloadService);
            Assert.NotNull(queueAdminService);
            Assert.NotNull(workflowBulkService);
            Assert.NotNull(taskService);
            Assert.NotNull(healthService);
            Assert.NotNull(metadataService);
            Assert.NotNull(workflowService);
        }
    }
}
