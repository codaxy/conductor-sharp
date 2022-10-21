using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class StringInterpolationInput : WorkflowInput<StringInterpolationOutput>
    {
        public string FirstInput { get; set; }
        public string SecondInput { get; set; }
    }

    public class StringInterpolationOutput : WorkflowOutput
    {
        public dynamic EmailBody { get; set; }
    }

    [OriginalName("TEST_StringInterpolation")]
    public class StringInterpolation : Workflow<StringInterpolationInput, StringInterpolationOutput>
    {
        public EmailPrepareV1 EmailPrepare { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<StringInterpolation>();

            builder.AddTask(
                wf => wf.EmailPrepare,
                wf =>
                    new()
                    {
                        Address = $"{wf.WorkflowInput.FirstInput},{wf.WorkflowInput.SecondInput}",
                        Name = $"Workflow name: {NamingUtil.NameOf<StringInterpolation>()}"
                    }
            );

            builder.SetOutput(a => new StringInterpolationOutput { EmailBody = a.EmailPrepare.Output.EmailBody });

            return builder.Build(opts => opts.Version = 1);
        }
    }
}
