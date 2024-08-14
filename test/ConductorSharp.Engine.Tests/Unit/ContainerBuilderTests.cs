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

            var alternateAdminService = container.GetKeyedService<IAdminService>(alternateClient);
            var alternateEventService = container.GetKeyedService<IEventService>(alternateClient);
            var alternateExternalPayloadService = container.GetKeyedService<IExternalPayloadService>(alternateClient);
            var alternateQueueAdminService = container.GetKeyedService<IQueueAdminService>(alternateClient);
            var alternateWorkflowBulkService = container.GetKeyedService<IWorkflowBulkService>(alternateClient);
            var alternateTaskService = container.GetKeyedService<ITaskService>(alternateClient);
            var alternateHealthService = container.GetKeyedService<IHealthService>(alternateClient);
            var alternateMetadataService = container.GetKeyedService<IMetadataService>(alternateClient);
            var alternateWorkflowService = container.GetKeyedService<IWorkflowService>(alternateClient);

            Assert.NotNull(adminService);
            Assert.NotNull(eventService);
            Assert.NotNull(externalPayloadService);
            Assert.NotNull(queueAdminService);
            Assert.NotNull(workflowBulkService);
            Assert.NotNull(taskService);
            Assert.NotNull(healthService);
            Assert.NotNull(metadataService);
            Assert.NotNull(workflowService);

            Assert.NotNull(alternateAdminService);
            Assert.NotNull(alternateEventService);
            Assert.NotNull(alternateExternalPayloadService);
            Assert.NotNull(alternateQueueAdminService);
            Assert.NotNull(alternateWorkflowBulkService);
            Assert.NotNull(alternateTaskService);
            Assert.NotNull(alternateHealthService);
            Assert.NotNull(alternateMetadataService);
            Assert.NotNull(alternateWorkflowService);

            Assert.NotEqual(adminService, alternateAdminService);
            Assert.NotEqual(eventService, alternateEventService);
            Assert.NotEqual(externalPayloadService, alternateExternalPayloadService);
            Assert.NotEqual(queueAdminService, alternateQueueAdminService);
            Assert.NotEqual(workflowBulkService, alternateWorkflowBulkService);
            Assert.NotEqual(taskService, alternateTaskService);
            Assert.NotEqual(healthService, alternateHealthService);
            Assert.NotEqual(metadataService, alternateMetadataService);
            Assert.NotEqual(workflowService, alternateWorkflowService);
        }
    }
}
