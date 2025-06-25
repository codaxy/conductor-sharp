using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Tests.Samples.Workers;
using ConductorSharp.Engine.Tests.Util;
using ConductorSharp.Engine.Util.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class TaskDefinitionBuilderTests
    {
        private readonly IServiceProvider _container;

        public TaskDefinitionBuilderTests()
        {
            var _containerBuilder = new ServiceCollection();

            _containerBuilder
                .AddConductorSharp("http://example.com/api")
                .AddExecutionManager(10, 100, 100, null)
                .AddPipelines(pipelines =>
                {
                    pipelines.AddContextLogging();
                    pipelines.AddRequestResponseLogging();
                    pipelines.AddValidation();
                });

            _container = _containerBuilder.BuildServiceProvider();
        }

        [Fact]
        public void ReturnsCorrectDefinition()
        {
            var taskDefinitionBuilder = _container.GetRequiredService<TaskDefinitionBuilder>();
            var definition = SerializationUtil.SerializeObject(taskDefinitionBuilder.Build<GetCustomerHandler>(null));
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Tasks/CustomerGet.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void ConfigurableBuilderReturnsCorrectDefinition()
        {
            var builder = new ServiceCollection();

            builder.AddConductorSharp(baseUrl: "http://empty/empty");

            builder.AddSingleton(new BuildConfiguration { DefaultOwnerApp = "owner" });

            builder.RegisterWorkerTask<GetCustomerHandler>();

            var container = builder.BuildServiceProvider();

            var definition = container.GetRequiredService<TaskDef>();

            Assert.Equal("owner", definition.OwnerApp);
        }
    }
}
