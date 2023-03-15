using ConductorSharp.Engine.Util.Builders;
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
        public object EmailBody { get; set; }
    }

    [OriginalName("TEST_StringInterpolation")]
    public class StringInterpolation : Workflow<StringInterpolation, StringInterpolationInput, StringInterpolationOutput>
    {
        public StringInterpolation(WorkflowDefinitionBuilder<StringInterpolation, StringInterpolationInput, StringInterpolationOutput> builder)
            : base(builder) { }

        public EmailPrepareV1 EmailPrepare { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.EmailPrepare,
                wf =>
                    new()
                    {
                        Address = $"{wf.WorkflowInput.FirstInput},{wf.WorkflowInput.SecondInput}",
                        Name = $"Workflow name: {NamingUtil.NameOf<StringInterpolation>()}"
                    }
            );

            _builder.SetOutput(a => new() { EmailBody = a.EmailPrepare.Output.EmailBody });
        }
    }
}
