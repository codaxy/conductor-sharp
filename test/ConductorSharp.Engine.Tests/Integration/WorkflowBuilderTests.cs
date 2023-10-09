using Autofac;
using Autofac.Core;
using ConductorSharp.Engine.Exceptions;
using ConductorSharp.Engine.Tests.Samples.Workflows;
using ConductorSharp.Engine.Tests.Util;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Patterns.Extensions;

namespace ConductorSharp.Engine.Tests.Integration
{
    public class WorkflowBuilderTests
    {
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

        [Fact]
        public void BuilderReturnsCorrectDefinitionPassthroughTaskWorkflow()
        {
            var definition = GetDefinitionFromWorkflow<PassthroughTaskWorkflow>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/PassthroughTaskWorkflow.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionHumanWorkflow()
        {
            var definition = GetDefinitionFromWorkflow<HumanTaskWorkflow>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/HumanTask.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionWaitTaskWorkflow()
        {
            var definition = GetDefinitionFromWorkflow<WaitTaskWorkflow>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/WaitTask.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionIndexerWorkflow()
        {
            var definition = GetDefinitionFromWorkflow<IndexerWorkflow>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/IndexerWorkflow.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionDefaultValueWorkflow()
        {
            var definition = GetDefinitionFromWorkflow<DefaultValueWorkflow>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/DefaultValueWorkflow.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionListInitalizationWorkflow()
        {
            var definition = GetDefinitionFromWorkflow<ListInitializationWorkflow>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/ListInitializationWorkflow.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionEvaluateExpressionWorkflow()
        {
            var definition = GetDefinitionFromWorkflow<EvaluateExpressionWorkflow>();
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/EvaluateExpressionWorkflow.json");

            Assert.Equal(expectedDefinition, definition);
        }

        [Fact]
        public void BuilderReturnsCorrectDefinitionNonEvaluatableWorkflow()
        {
            var exception = Assert.Throws<DependencyResolutionException>(GetDefinitionFromWorkflow<NonEvaluatableWorkflow>);
            Assert.IsType<NonEvaluatableExpressionException>(exception.InnerException);
        }

        private string GetDefinitionFromWorkflow<TWorkflow>() where TWorkflow : IConfigurableWorkflow
        {
            var workflow = RegisterWorkflow<TWorkflow>()
                .Resolve<IEnumerable<WorkflowDefinition>>()
                .First(a => a.Name == NamingUtil.NameOf<TWorkflow>());

            return SerializationUtil.SerializeObject(workflow);
        }

        private IContainer RegisterWorkflow<TWorkflow>() where TWorkflow : IConfigurableWorkflow
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder
                .AddConductorSharp("example.com", "api", false)
                .AddExecutionManager(10, 100, 100, null, typeof(WorkflowBuilderTests).Assembly)
                .AddPipelines(pipelines =>
                {
                    pipelines.AddContextLogging();
                    pipelines.AddRequestResponseLogging();
                    pipelines.AddValidation();
                })
                .AddCSharpLambdaTasks("TEST");

            containerBuilder.RegisterWorkflow<TWorkflow>();
            return containerBuilder.Build();
        }
    }
}
