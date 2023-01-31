using ConductorSharp.Engine.Tests.Samples.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class UntypedPropertyInput : WorkflowInput<UntypedPropertyOutput> { }

    public class UntypedPropertyOutput : WorkflowOutput { }

    public class UntypedProperty : Workflow<UntypedProperty, UntypedPropertyInput, UntypedPropertyOutput>
    {
        public UntypedProperty(WorkflowDefinitionBuilder<UntypedProperty, UntypedPropertyInput, UntypedPropertyOutput> builder) : base(builder) { }

        public CustomerGetV1 GetCustomer { get; set; }
        public PrepareEmailHandler PrepareEmail { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.GetCustomer, wf => new() { CustomerId = 1 });

            _builder.AddTask(
                wf => wf.PrepareEmail,
                wf =>
                    new PrepareEmailRequest()
                    {
                        CustomerName = wf.GetCustomer.Output.FullName.FirstName,
                        Address = wf.GetCustomer.Output.AddressString
                    }
            );
        }
    }
}
