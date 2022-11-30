using Autofac;
using ConductorSharp.Engine.Builders.Configurable;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Tests.Samples.Workflows;
using ConductorSharp.Engine.Util.Builders;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class ContainerBuilderTests
    {
        [Fact]
        public void RegisterDefinition()
        {
            var builder = new ContainerBuilder();

            builder.RegisterWorkflow<StringInterpolation>(new BuildConfiguration());
            builder.RegisterWorkflow<PatternTasks>();
            var container = builder.Build();

            var definitions = container.Resolve<IEnumerable<WorkflowDefinition>>().ToList();

            Assert.True(definitions.Any());
        }
    }
}
