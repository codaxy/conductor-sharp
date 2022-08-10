using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Model;
using MediatR;

namespace ConductorSharp.Definitions.Workflows
{
    public class CSharpSimpleTaskWorkflowInput : WorkflowInput<CSharpSimpleTaskWorkflowOutput>
    {
        public string Input { get; set; }
    }

    public class CSharpSimpleTaskWorkflowOutput : WorkflowOutput { }

    public class CSharpSimpleTaskWorkflow : Workflow<CSharpSimpleTaskWorkflowInput, CSharpSimpleTaskWorkflowOutput>
    {
        public class CSharpSimpleTaskInput : IRequest<CSharpSimpleTaskOutput>
        {
            public string Input { get; set; }
        }

        public class CSharpSimpleTaskOutput
        {
            public string Output { get; set; }
        }

        public SimpleTaskModel<CSharpSimpleTaskInput, CSharpSimpleTaskOutput> CSharpSimpleTask { get; set; }
        public SimpleTaskModel<CSharpSimpleTaskInput, CSharpSimpleTaskOutput> CSharpSimpleTask2 { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<CSharpSimpleTaskWorkflow>();
            builder.AddTask(
                wf => wf.CSharpSimpleTask,
                wf => new() { Input = wf.WorkflowInput.Input },
                input =>
                {
                    var output = new CSharpSimpleTaskOutput { Output = input.Input.Length < 5 ? "<5" : "> =5" };

                    return output;
                }
            );

            builder.AddTask(
                wf => wf.CSharpSimpleTask2,
                wf => new() { Input = wf.WorkflowInput.Input },
                input =>
                {
                    var output = new CSharpSimpleTaskOutput { Output = input.Input.Length < 5 ? "<5" : ">=5" };

                    return output;
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
