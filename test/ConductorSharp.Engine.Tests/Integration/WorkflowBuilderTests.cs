using ConductorSharp.Engine.Tests.Samples.Workflows;

namespace ConductorSharp.Engine.Tests.Integration
{
    public class WorkflowBuilderTests
    {
        [Fact]
        public void BuilderReturnsCorrectDefinition()
        {
            var definition = JsonConvert.SerializeObject(new SendCustomerNotification().GetDefinition(), Formatting.Indented);
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/SendCustomerNotification.json");

            Assert.Equal(expectedDefinition, definition);
        }
    }
}
