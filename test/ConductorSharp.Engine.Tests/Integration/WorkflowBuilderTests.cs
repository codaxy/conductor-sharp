using Autofac;
using ConductorSharp.Engine.Tests.Samples.Workflows;
using ConductorSharp.Engine.Tests.Util;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Patterns.Extensions;

namespace ConductorSharp.Engine.Tests.Integration
{
    public class WorkflowBuilderTests
    {
        private readonly IContainer _container;

        public WorkflowBuilderTests()
        {
            var _containerBuilder = new ContainerBuilder();

            _containerBuilder
                .AddConductorSharp("example.com", "api", false)
                .AddExecutionManager(10, 100, 100, null, typeof(WorkflowBuilderTests).Assembly)
                .AddPipelines(pipelines =>
                {
                    pipelines.AddContextLogging();
                    pipelines.AddRequestResponseLogging();
                    pipelines.AddValidation();
                })
                .AddCSharpLambdaTasks("TEST");

            _containerBuilder.RegisterWorkflow<SendCustomerNotification>();
            _containerBuilder.RegisterWorkflow<StringInterpolation>();
            _containerBuilder.RegisterWorkflow<Samples.Workflows.NestedObjects>();
            _containerBuilder.RegisterWorkflow<TaskInputInitializationNew>();
            _containerBuilder.RegisterWorkflow<TaskInputInitializationMemberInit>();

            _containerBuilder.RegisterWorkflow<Arrays>();
            _containerBuilder.RegisterWorkflow<ConditionallySendCustomerNotification>();
            _containerBuilder.RegisterWorkflow<DynamicTask>();
            _containerBuilder.RegisterWorkflow<OptionalTaskWorkflow>();
            _containerBuilder.RegisterWorkflow<TerminateTaskWorfklow>();
            _containerBuilder.RegisterWorkflow<DecisionInDecision>();
            _containerBuilder.RegisterWorkflow<ScaffoldedWorkflows>();
            _containerBuilder.RegisterWorkflow<VersionAttributeWorkflow>();
            _containerBuilder.RegisterWorkflow<PatternTasks>();
            _containerBuilder.RegisterWorkflow<CastWorkflow>();
            _containerBuilder.RegisterWorkflow<StringAddition>();
            _containerBuilder.RegisterWorkflow<CSharpLambdaWorkflow>();
            _containerBuilder.RegisterWorkflow<DecisionTask>();
            _containerBuilder.RegisterWorkflow<SwitchTask>();

            _container = _containerBuilder.Build();
        }

        [Fact]
        public void BuilderReturnsCorrectDefinition()
        {
            var definition = GetDefinitionFromWorkflow<SendCustomerNotification>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/SendCustomerNotification.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionStringInterpolation()
        {
            var definition = GetDefinitionFromWorkflow<StringInterpolation>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/StringInterpolation.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionNestedObjects()
        {
            var definition = GetDefinitionFromWorkflow<Samples.Workflows.NestedObjects>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/NestedObjects.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuildersReturnSameDefinitionsTaskInitialization()
        {
            var explicitDef = GetDefinitionFromWorkflow<TaskInputInitializationNew>();
            var memberInitDef = GetDefinitionFromWorkflow<TaskInputInitializationMemberInit>();

            Assert.True(explicitDef == memberInitDef, "Definitions are not equal");
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionArrayTask()
        {
            var definition = GetDefinitionFromWorkflow<Arrays>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/Arrays.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionSubworkflowInDecision()
        {
            var definition = GetDefinitionFromWorkflow<ConditionallySendCustomerNotification>();

            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile(
                "~/Samples/Workflows/ConditionallySendCustomerNotificationOutput.json"
            );

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDynamicTask()
        {
            var definition = GetDefinitionFromWorkflow<DynamicTask>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/DynamicTask.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionOptionalTask()
        {
            var definition = GetDefinitionFromWorkflow<OptionalTaskWorkflow>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/OptionalTaskWorkflow.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionTerminateTask()
        {
            var definition = GetDefinitionFromWorkflow<TerminateTaskWorfklow>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/TerminateTaskWorkflow.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionDecisionInDecision()
        {
            var definition = GetDefinitionFromWorkflow<DecisionInDecision>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/DecisionInDecision.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionSubWorkflowModelsOnly()
        {
            var definition = GetDefinitionFromWorkflow<ScaffoldedWorkflows>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/ScaffoldedWorkflows.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionSubworkflowVersionAttribute()
        {
            var definition = GetDefinitionFromWorkflow<VersionAttributeWorkflow>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/VersionAttributeWorkflow.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionPatternTasks()
        {
            var definition = GetDefinitionFromWorkflow<PatternTasks>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/PatternTasks.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionCastWorkflow()
        {
            var definition = GetDefinitionFromWorkflow<CastWorkflow>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/CastWorkflow.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionStringAddition()
        {
            var definition = GetDefinitionFromWorkflow<StringAddition>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/StringAddition.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionCSharpLambdaWorfklow()
        {
            var definition = GetDefinitionFromWorkflow<CSharpLambdaWorkflow>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/CSharpLambdaWorkflow.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionDecisionTask()
        {
            var definition = GetDefinitionFromWorkflow<DecisionTask>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/DecisionTask.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionSwitchTask()
        {
            var definition = GetDefinitionFromWorkflow<SwitchTask>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/SwitchTask.json");

            Assert.Equal(expectedDefinition, definition);
        }

        private string GetDefinitionFromWorkflow<TNameable>() where TNameable : INameable
        {
            var workflow = _container.Resolve<IEnumerable<WorkflowDefinition>>().First(a => a.Name == NamingUtil.NameOf<TNameable>());

            return SerializationUtil.SerializeObject(workflow);
        }
    }
}
