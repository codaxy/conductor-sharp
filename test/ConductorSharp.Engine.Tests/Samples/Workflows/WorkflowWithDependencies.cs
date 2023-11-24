using ConductorSharp.Engine.Tests.Util;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class WorkflowWithDependenciesInput : WorkflowInput<WorkflowWithDependenciesOutput> { }

    public class WorkflowWithDependenciesOutput : WorkflowOutput { }

    public class WorkflowWithDependencies : Workflow<WorkflowWithDependencies, WorkflowWithDependenciesInput, WorkflowWithDependenciesOutput>
    {
        private readonly IConfigurationService _configurationService;

        public CustomerGetV1 GetCustomer { get; set; }

        public WorkflowWithDependencies(
            IConfigurationService configurationService,
            WorkflowDefinitionBuilder<WorkflowWithDependencies, WorkflowWithDependenciesInput, WorkflowWithDependenciesOutput> builder
        ) : base(builder)
        {
            _configurationService = configurationService;
        }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.GetCustomer, wf => new() { CustomerId = _configurationService.GetValue<int>("CustomerId") });
        }
    }
}
