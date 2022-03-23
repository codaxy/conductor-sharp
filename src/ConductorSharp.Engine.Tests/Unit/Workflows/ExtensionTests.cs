using Autofac;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Extensions;
using System.Collections.Generic;
using Xunit;

namespace ConductorSharp.Engine.Tests.Unit.Workflows;

public class ExtensionTests
{
    [Fact]
    public void ShouldRegisterWorkflowDefinitionFromFile()
    {
        var builder = new ContainerBuilder();

        builder.RegisterWorkflowDefinition("Infrastructure/Files/definition.json");

        var container = builder.Build();

        var definitions = container.Resolve<IEnumerable<WorkflowDefinition>>();

        Assert.Single(definitions);
    }
}
