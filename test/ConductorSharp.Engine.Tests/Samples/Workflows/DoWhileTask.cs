namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public sealed class DoWhileTaskInput : WorkflowInput<DoWhileTaskOutput>
    {
        public int Loops { get; set; }
    }

    public sealed class DoWhileTaskOutput : WorkflowOutput { }

    public sealed class DoWhileTask : Workflow<DoWhileTask, DoWhileTaskInput, DoWhileTaskOutput>
    {
        public DoWhileTaskModel DoWhile { get; set; }
        public CustomerGetV1 GetCustomer { get; set; }

        public DoWhileTask(WorkflowDefinitionBuilder<DoWhileTask, DoWhileTaskInput, DoWhileTaskOutput> builder)
            : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.DoWhile,
                wf => new() { Value = wf.Input.Loops },
                "$.do_while.iteration < $.value",
                builder =>
                {
                    builder.AddTask(wf => wf.GetCustomer, wf => new CustomerGetV1Input() { CustomerId = "CUSTOMER-1" });
                }
            );
        }
    }
}
