using Autofac;
using ConductorSharp.Engine.Tests.Samples.Workers;
using ConductorSharp.Engine.Tests.Util;
using ConductorSharp.Engine.Util.Builders;
using ConductorSharp.Engine.Extensions;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class TaskDefinitionBuilderTests
    {
        private readonly IContainer _container;

        public TaskDefinitionBuilderTests()
        {
            var _containerBuilder = new ContainerBuilder();

            _containerBuilder
                .AddConductorSharp("example.com", "api", false)
                .AddExecutionManager(10, 100, 100)
                .AddPipelines(pipelines =>
                {
                    pipelines.AddContextLogging();
                    pipelines.AddRequestResponseLogging();
                    pipelines.AddValidation();
                });

            _container = _containerBuilder.Build();
        }

        [Fact]
        public void ReturnsCorrectDefinition()
        {
            var taskDefinitionBuilder = _container.Resolve<TaskDefinitionBuilder>();
            var definition = SerializationUtil.SerializeObject(taskDefinitionBuilder.Build<GetCustomerHandler>(null));
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
