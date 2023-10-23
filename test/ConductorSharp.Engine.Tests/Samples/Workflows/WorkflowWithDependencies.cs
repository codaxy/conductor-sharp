using ConductorSharp.Engine.Tests.Util;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class WorkflowWithDependenciesInput : WorkflowInput<WorkflowWithDependenciesOutput> { }

    public class WorkflowWithDependenciesOutput : WorkflowOutput { }

    public class WorkflowWithDependencies : Workflow<WorkflowWithDependencies, WorkflowWithDependenciesInput, WorkflowWithDependenciesOutput>
    {
        private readonly IConfigurationService _testService;

        public WorkflowWithDependencies(
            IConfigurationService testService,
            WorkflowDefinitionBuilder<WorkflowWithDependencies, WorkflowWithDependenciesInput, WorkflowWithDependenciesOutput> builder
        ) : base(builder)
        {
            _testService = testService;
        }

        public override void BuildDefinition()
        {
            _b
        }
    }
}
