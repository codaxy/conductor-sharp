using Autofac;
using ConductorSharp.Engine.Builders.Configurable;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Tests.Samples.Workflows;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class ContainerBuilderTests
    {
        [Fact]
        public void RegisterDefinition()
        {
            var builder = new ContainerBuilder();

            builder.RegisterWorkflow<StringInterpolation>(new BuildConfiguration(), new BuildContext());
            builder.RegisterWorkflow<PatternTasks>();
            var container = builder.Build();

            var definitions = container.Resolve<IEnumerable<WorkflowDefinition>>().ToList();

            Assert.Single(definitions);
        }
    }
}
