using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows;

public class InvalidFormatterArgumentWorkflowInput : WorkflowInput<InvalidFormatterArgumentWorkflowOutput>;

public class InvalidFormatterArgumentWorkflowOutput : WorkflowOutput;

internal class InvalidFormatterArgumentWorkflow(
    WorkflowDefinitionBuilder<InvalidFormatterArgumentWorkflow, InvalidFormatterArgumentWorkflowInput, InvalidFormatterArgumentWorkflowOutput> builder
) : Workflow<InvalidFormatterArgumentWorkflow, InvalidFormatterArgumentWorkflowInput, InvalidFormatterArgumentWorkflowOutput>(builder)
{
    [ConductorExpressionFormatter]
    public static string Format([FormatterParameter] string input) => input;

    public EmailPrepareV1 EmailPrepare { get; set; }

    public override void BuildDefinition()
    {
        _builder.AddTask(wf => wf.EmailPrepare, wf => new() { Name = Format("Invalid argument") });
    }
}
