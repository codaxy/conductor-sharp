using ConductorSharp.Engine.Builders.Configurable;
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
        public dynamic EmailBody { get; set; }
    }

    [OriginalName("TEST_StringInterpolation")]
    public class StringInterpolation : Workflow<StringInterpolation, StringInterpolationInput, StringInterpolationOutput>
    {
        public StringInterpolation(BuildConfiguration buildConfiguration, BuildContext buildContext) : base(buildConfiguration, buildContext) { }

        public EmailPrepareV1 EmailPrepare { get; set; }

        public override void AddTasks(WorkflowDefinitionBuilder<StringInterpolation, StringInterpolationInput, StringInterpolationOutput> builder)
        {
            //_builder.AddTask(
            //    wf => wf.EmailPrepare,
            //    wf =>
            //        new()
            //        {
            //            Address = $"{wf.WorkflowInput.FirstInput},{wf.WorkflowInput.SecondInput}",
            //            Name = $"Workflow name: {NamingUtil.NameOf<StringInterpolation>()}"
            //        }
            //);

            builder.SetOutput(a => new() { EmailBody = a.EmailPrepare.Output.EmailBody });
        }
    }
}
