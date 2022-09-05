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
            var explicitDef = SerializationUtil.SerializeObject(new TaskInputInitializationNew().GetDefinition());
            var memberInitDef = SerializationUtil.SerializeObject(new TaskInputInitializationMemberInit().GetDefinition());

            Assert.True(explicitDef == memberInitDef, "Definitions are not equal");
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionArrayTask()
        {
            var definition = SerializationUtil.SerializeObject(new Arrays().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/Arrays.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionSubworkflowInDecision()
        {
            var definition = SerializationUtil.SerializeObject(new ConditionallySendCustomerNotification().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile(
                "~/Samples/Workflows/ConditionallySendCustomerNotificationOutput.json"
            );

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionOptionalTask()
        {
            var definition = SerializationUtil.SerializeObject(new OptionalTaskWorkflow().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/OptionalTaskWorkflow.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionTerminateTask()
        {
            var definition = SerializationUtil.SerializeObject(new TerminateTaskWorfklow().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/TerminateTaskWorkflow.json");

            Assert.Equal(expectedDefinition, definition);
        }
    }
}
