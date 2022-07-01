using ConductorSharp.Engine.Tests.Samples.Workers;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class TaskDefinitionBuilderTests
    {
        [Fact]
        public void ReturnsCorrectDefinition()
        {
            var definition = JsonConvert.SerializeObject(TaskDefinitionBuilder.Build<GetCustomerHandler>(null), Formatting.Indented);
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Tasks/CustomerGet.json");

            Assert.Equal("abc", definition);
        }
    }
}
