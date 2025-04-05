namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class StaticFormatterInput : WorkflowInput<StaticFormatterOutput>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class StaticFormatterOutput : WorkflowOutput
    {
        public object EmailBody { get; set; }
    }

    public class StaticFormatter : Workflow<StaticFormatter, StaticFormatterInput, StaticFormatterOutput>
    {
        public StaticFormatter(WorkflowDefinitionBuilder<StaticFormatter, StaticFormatterInput, StaticFormatterOutput> builder)
            : base(builder) { }

        public EmailPrepareV1 EmailPrepare { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.EmailPrepare,
                wf =>
                    new()
                    {
                        Address = SmartEmailConverter.Format(wf.Input.FirstName, wf.Input.LastName),
                        Name = $"Workflow name: {NamingUtil.NameOf<StringInterpolation>()}"
                    }
            );

            _builder.SetOutput(a => new() { EmailBody = a.EmailPrepare.Output.EmailBody });
        }

        public static EmailConverter SmartEmailConverter => new EmailConverter();

        public class EmailConverter
        {
            public string Format(string firstName, string lastName) => $"{firstName}.{lastName}@example.com";
        }
    }
}
