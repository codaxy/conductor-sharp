using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Builders;

namespace ConductorSharp.ApiEnabled.Handlers;

public class ExampleWorkflowInput : WorkflowInput<ExampleWorkflowOutput>
{
}

public class ExampleWorkflowOutput : WorkflowOutput
{
}
public class ExampleWorkflow : Workflow<ExampleWorkflowInput, ExampleWorkflowOutput>
{
    public override WorkflowDefinition GetDefinition()
    {
        var builder = new WorkflowDefinitionBuilder<ExampleWorkflow>();

        return builder.Build(
            options =>
            {
                options.Version = 1;
            }
        );
    }
}
