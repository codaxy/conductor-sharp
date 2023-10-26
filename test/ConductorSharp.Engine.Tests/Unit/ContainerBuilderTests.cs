using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Tests.Samples.Workflows;
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
                .AddConductorSharp(baseUrl: "empty", apiPath: "empty")
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

            var definitions = container.GetRequiredService<IEnumerable<WorkflowDefinition>>().ToList();

            Assert.True(definitions.All(a => a.OwnerApp == "testApp"));
        }

        [Fact]
        public void OverridesBuildConfigurationDefaults()
        {
            var builder = new ServiceCollection();
            var overrideValue = "override";
            builder
                .AddConductorSharp(baseUrl: "empty", apiPath: "empty")
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

            var definitions = container.GetRequiredService<IEnumerable<WorkflowDefinition>>().ToList();

            Assert.True(definitions.All(a => a.OwnerApp == overrideValue));
        }
    }
}
