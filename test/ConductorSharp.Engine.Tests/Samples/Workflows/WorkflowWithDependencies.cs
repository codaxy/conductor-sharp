using ConductorSharp.Engine.Tests.Util;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class WorkflowWithDependenciesInput : WorkflowInput<WorkflowWithDependenciesOutput> { }

    public class WorkflowWithDependenciesOutput : WorkflowOutput { }

    public class WorkflowWithDependencies : Workflow<WorkflowWithDependencies, WorkflowWithDependenciesInput, WorkflowWithDependenciesOutput>
    {
        public WorkflowWithDependencies(
            ITestService testService,
            WorkflowDefinitionBuilder<WorkflowWithDependencies, WorkflowWithDependenciesInput, WorkflowWithDependenciesOutput> builder
        ) : base(builder) { }
    }
}
