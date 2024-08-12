using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConductorSharp.Engine.Builders.Metadata;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class StringAdditionInput : WorkflowInput<StringAdditionOutput>
    {
        public string Input { get; set; }
    }

    public class StringAdditionOutput : WorkflowOutput { }

    [OriginalName("string_addition")]
    public class StringAddition : Workflow<StringAddition, StringAdditionInput, StringAdditionOutput>
    {
        public StringAddition(WorkflowDefinitionBuilder<StringAddition, StringAdditionInput, StringAdditionOutput> builder)
            : base(builder) { }

        public class StringTaskOutput { }

        public class StringTaskInput : IRequest<StringTaskOutput>
        {
            public string Input { get; set; }
        }

        public LambdaTaskModel<StringTaskInput, StringTaskOutput> StringTask { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.StringTask,
                wf =>
                    new()
                    {
                        Input =
                            1
                            + "Str_"
                            + "2Str_"
                            + wf.WorkflowInput.Input
                            + $"My input: {wf.WorkflowInput.Input}"
                            + NamingUtil.NameOf<StringAddition>()
                            + 1
                    },
                ""
            );
        }
    }
}
