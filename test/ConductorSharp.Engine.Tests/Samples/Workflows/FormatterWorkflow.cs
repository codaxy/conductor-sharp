namespace ConductorSharp.Engine.Tests.Samples.Workflows;

public class FormatterWorkflowInput : WorkflowInput<FormatterWorkflowOutput>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class FormatterWorkflowOutput : WorkflowOutput { }

public class FormatterWorkflow : Workflow<FormatterWorkflow, FormatterWorkflowInput, FormatterWorkflowOutput>
{
    public class Library
    {
        public static Library Singleton { get; } = new();

        [ConductorExpressionFormatter]
        public string PrepareEmail([FormatterParameter] string firstName, [FormatterParameter] string lastName) =>
            $"{firstName}.{lastName}@example.com";

        [ConductorExpressionFormatter]
        public static string FormatName([FormatterParameter] string firstName, [FormatterParameter] string lastName, string prefix) =>
            $"{prefix} {firstName} {lastName}";
    }

    public EmailPrepareV1 EmailPrepare { get; set; }

    public FormatterWorkflow(WorkflowDefinitionBuilder<FormatterWorkflow, FormatterWorkflowInput, FormatterWorkflowOutput> builder)
        : base(builder) { }

    public override void BuildDefinition()
    {
        _builder.AddTask(
            wf => wf.EmailPrepare,
            wf =>
                new()
                {
                    Address = Library.Singleton.PrepareEmail(wf.Input.FirstName, wf.Input.LastName),
                    Name = Library.FormatName(wf.Input.FirstName, wf.Input.LastName, "Mr")
                }
        );
    }
}
