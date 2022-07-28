using ConductorSharp.Engine.Tests.Samples.Workflows;
using ConductorSharp.Engine.Tests.Util;

namespace ConductorSharp.Engine.Tests.Integration
{
    public class WorkflowBuilderTests
    {
        [Fact]
        public void BuilderReturnsCorrectDefinition()
        {
            var definition = SerializationUtil.SerializeObject(new SendCustomerNotification().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/SendCustomerNotification.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionStringInterpolation()
        {
            var definition = SerializationUtil.SerializeObject(new StringInterpolation().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/StringInterpolation.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionNestedObjects()
        {
            var definition = SerializationUtil.SerializeObject(new Samples.Workflows.NestedObjects().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/NestedObjects.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuildersReturnSameDefinitionsTaskInitialization()
        {
            var explicitDef = SerializationUtil.SerializeObject(new TaskInputInitializationExplicit().GetDefinition());
            var targetTypedDef = SerializationUtil.SerializeObject(new TaskInputInitializationTargetTyped().GetDefinition());

            Assert.True(explicitDef == targetTypedDef, "Definitions are not equal");
        }
    }
}
