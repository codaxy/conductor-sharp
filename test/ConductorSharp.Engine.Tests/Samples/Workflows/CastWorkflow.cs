using ConductorSharp.Engine.Tests.Samples.Workers;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class CastWorkflowInput : WorkflowInput<CastWorkflowOutput> { }

    public class CastWorkflowOutput : WorkflowOutput { }

    public class CastWorkflow : Workflow<CastWorkflow, CastWorkflowInput, CastWorkflowOutput>
    {
        public CastWorkflow(WorkflowDefinitionBuilder<CastWorkflow, CastWorkflowInput, CastWorkflowOutput> builder) : base(builder) { }

        public CustomerGetV1 GetCustomer { get; set; }
        public PrepareEmailHandler PrepareEmail { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.GetCustomer, wf => new() { CustomerId = 1 });

            _builder.AddTask(
                wf => wf.PrepareEmail,
                wf =>
                    new PrepareEmailRequest
                    {
                        CustomerName = ((FullName)wf.GetCustomer.Output.Name).FirstName,
                        Address = (string)wf.GetCustomer.Output.Address
                    }
            );
        }
    }
}
