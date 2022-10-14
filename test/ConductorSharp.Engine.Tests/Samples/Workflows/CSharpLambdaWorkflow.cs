using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class CSharpLambdaWorkflowInput : WorkflowInput<CSharpLambdaWorkflowOutput>
    {
        public string InputProp { get; set; }
    }

    public class CSharpLambdaWorkflowOutput : WorkflowOutput { }

    public class CSharpLambdaWorkflow : Workflow<CSharpLambdaWorkflowInput, CSharpLambdaWorkflowOutput>
    {
        public class TaskInput : IRequest<TaskOutput>
        {
            public string InputProp { get; set; }
        }

        public class TaskOutput
        {
            public string OutputProp { get; set; }
        }

        public CSharpLambdaTaskModel<TaskInput, TaskOutput> FirstTask { get; set; }
        public CSharpLambdaTaskModel<TaskInput, TaskOutput> SecondTask { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<CSharpLambdaWorkflow>();

            builder.AddTask(
                wf => wf.FirstTask,
                wf => new() { InputProp = wf.WorkflowInput.InputProp },
                input => new TaskOutput { OutputProp = input.InputProp }
            );

            builder.AddTask(
                wf => wf.SecondTask,
                wf => new() { InputProp = wf.FirstTask.Output.OutputProp },
                input => new TaskOutput { OutputProp = input.InputProp }
            );

            Lambdas = builder.Lambdas;

            return builder.Build(opts => opts.Version = 1);
        }
    }
}
