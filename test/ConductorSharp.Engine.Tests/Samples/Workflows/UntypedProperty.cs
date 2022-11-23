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

    public class UntypedProperty : Workflow<UntypedPropertyInput, UntypedPropertyOutput>
    {
        public CustomerGetV1 GetCustomer { get; set; }
        public PrepareEmailHandler PrepareEmail { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<UntypedProperty>();

            builder.AddTask(wf => wf.GetCustomer, wf => new() { CustomerId = 1 });

            builder.AddTask(
                wf => wf.PrepareEmail,
                wf =>
                    new PrepareEmailRequest()
                    {
                        CustomerName = wf.GetCustomer.Output.FullName.FirstName,
                        Address = wf.GetCustomer.Output.AddressString
                    }
            );

            return builder.Build(opts =>
            {
                opts.Version = 1;
                opts.OwnerEmail = "test@test.com";
            });
        }
    }
}
