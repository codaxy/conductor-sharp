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
        public void BuilderReturnsCorrectDynamicTask()
        {
            var definition = SerializationUtil.SerializeObject(new DynamicTask().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/DynamicTask.json");

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

        [Fact]
        public void BuilderReturnsCorrectDefinitionDecisionInDecision()
        {
            var definition = SerializationUtil.SerializeObject(new DecisionInDecision().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/DecisionInDecision.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionSubWorkflowModelsOnly()
        {
            var definition = SerializationUtil.SerializeObject(new ScaffoldedWorkflows().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/ScaffoldedWorkflows.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionSubworkflowVersionAttribute()
        {
            var definition = SerializationUtil.SerializeObject(new VersionAttributeWorkflow().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/VersionAttributeWorkflow.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionPatternTasks()
        {
            var definition = SerializationUtil.SerializeObject(new PatternTasks().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/PatternTasks.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionUntypedProperty()
        {
            var definition = SerializationUtil.SerializeObject(new UntypedProperty().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/UntypedProperty.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionStringAddition()
        {
            var definition = SerializationUtil.SerializeObject(new StringAddition().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/StringAddition.json");

            Assert.Equal(expectedDefinition, definition);
        }
    }
}
