using Autofac;
using ConductorSharp.Engine.Tests.Samples.Workers;
using ConductorSharp.Engine.Tests.Util;
using ConductorSharp.Engine.Util.Builders;
using ConductorSharp.Engine.Extensions;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class TaskDefinitionBuilderTests
    {
        [Fact]
        public void ReturnsCorrectDefinition()
        {
            var definition = SerializationUtil.SerializeObject(TaskDefinitionBuilder.Build<GetCustomerHandler>(null));
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Tasks/CustomerGet.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void ConfigurableBuilderReturnsCorrectDefinition()
        {
            var builder = new ContainerBuilder();

            builder.AddConductorSharp(baseUrl: "empty", apiPath: "empty");

            builder.RegisterInstance(new BuildConfiguration { DefaultOwnerApp = "owner" });

            builder.RegisterWorkerTask<GetCustomerHandler>();

            var container = builder.Build();

            var definition = container.Resolve<TaskDefinition>();

            Assert.Equal("owner", definition.OwnerApp);
        }
    }
}
