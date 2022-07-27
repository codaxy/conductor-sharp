using ConductorSharp.Engine.Tests.Samples.Workers;
using ConductorSharp.Engine.Tests.Util;

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
    }
}
