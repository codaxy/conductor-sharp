namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public sealed class DoWhileTaskInput : WorkflowInput<DoWhileTaskOutput> { }

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
                wf => new() { LoopCondition = "$.do_while.iteration < 3" },
                builder =>
                {
                    builder.AddTask(wf => wf.GetCustomer, wf => new CustomerGetV1Input() { CustomerId = "CUSTOMER-1" });
                }
            );
        }
    }
}
