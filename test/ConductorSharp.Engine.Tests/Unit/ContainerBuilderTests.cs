﻿using ConductorSharp.Engine.Extensions;
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
                .AddConductorSharp(baseUrl: "empty", apiPath: "empty")
                .SetBuildConfiguration(
                    new BuildConfiguration
                    {
                        DefaultOwnerApp = "testApp",
                        DefaultOwnerEmail = "owner@test.app",
                        WorkflowPrefix = "TEST_APP_"
                    }
                )
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
                .SetBuildConfiguration(
                    new BuildConfiguration
                    {
                        DefaultOwnerApp = "testApp",
                        DefaultOwnerEmail = "owner@test.app",
                        WorkflowPrefix = "TEST_APP_"
                    }
                )
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

        [Fact]
        public void ResolveWorkflowDependencies()
        {
            var builder = new ServiceCollection();
            builder
                .AddConductorSharp(baseUrl: "empty", apiPath: "empty")
                .AddExecutionManager(
                    maxConcurrentWorkers: 1,
                    sleepInterval: 1,
                    longPollInterval: 1,
                    null,
                    handlerAssemblies: typeof(ContainerBuilderTests).Assembly
                );

            builder.AddTransient<ITestService, TestService>();
            builder.RegisterWorkflow<WorkflowWithDependencies>();
            var container = builder.BuildServiceProvider();
            var definitions = container.GetRequiredService<IEnumerable<WorkflowDefinition>>();
            Assert.Contains(definitions, def => def.Name == NamingUtil.NameOf<WorkflowWithDependencies>());
        }

        [Fact]
        public void FailsToResolveWorkflowDependencies()
        {
            var builder = new ServiceCollection();
            builder
                .AddConductorSharp(baseUrl: "empty", apiPath: "empty")
                .AddExecutionManager(
                    maxConcurrentWorkers: 1,
                    sleepInterval: 1,
                    longPollInterval: 1,
                    null,
                    handlerAssemblies: typeof(ContainerBuilderTests).Assembly
                );

            builder.RegisterWorkflow<WorkflowWithDependencies>();
            var container = builder.BuildServiceProvider();
            Assert.Throws<InvalidOperationException>(() => container.GetRequiredService<IEnumerable<WorkflowDefinition>>());
        }
    }
}
