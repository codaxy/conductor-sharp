using Autofac;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Tests.Samples.Workflows;
using ConductorSharp.Engine.Util.Builders;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class ContainerBuilderTests
    {
        [Fact]
        public void AppliesBuildConfigurationDefaults()
        {
            var builder = new ContainerBuilder();

            builder
                .AddConductorSharp(baseUrl: "empty", apiPath: "empty")
                .SetBuildConfiguration(
                    new BuildConfiguration
                    {
                        DefaultOwnerApp = "testApp",
                        DefaultOwnerEmail = "owner@test.app",
                        WorkflowPrefix = "TEST_APP_"
                    }
                )
                .AddExecutionManager(maxConcurrentWorkers: 1, sleepInterval: 1, longPollInterval: 1);

            builder.RegisterWorkflow<StringInterpolation>();
            var container = builder.Build();

            var definitions = container.Resolve<IEnumerable<WorkflowDefinition>>().ToList();

            Assert.True(definitions.All(a => a.OwnerApp == "testApp"));
        }

        [Fact]
        public void OverridesBuildConfigurationDefaults()
        {
            var builder = new ContainerBuilder();
            var overrideValue = "override";
            builder
                .AddConductorSharp(baseUrl: "empty", apiPath: "empty")
                .SetBuildConfiguration(
                    new BuildConfiguration
                    {
                        DefaultOwnerApp = "testApp",
                        DefaultOwnerEmail = "owner@test.app",
                        WorkflowPrefix = "TEST_APP_"
                    }
                )
                .AddExecutionManager(maxConcurrentWorkers: 1, sleepInterval: 1, longPollInterval: 1);

            builder.RegisterWorkflow<StringInterpolation>(new BuildConfiguration { DefaultOwnerApp = overrideValue });
            var container = builder.Build();

            var definitions = container.Resolve<IEnumerable<WorkflowDefinition>>().ToList();

            Assert.True(definitions.All(a => a.OwnerApp == overrideValue));
        }
    }
}
