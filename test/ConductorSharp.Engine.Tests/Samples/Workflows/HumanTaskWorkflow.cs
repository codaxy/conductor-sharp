using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class HumanTaskWorkflowInput : WorkflowInput<HumanTaskWorkflowOutput> { }

    public class HumanTaskWorkflowOutput : WorkflowOutput { }

    public class HumanTaskWorkflow : Workflow<HumanTaskWorkflow, HumanTaskWorkflowInput, HumanTaskWorkflowOutput>
    {
        public class HumanTaskOutput
        {
            public string CustomerId { get; set; }
        }

        public HumanTaskModel<HumanTaskOutput> HumanTask { get; set; }
        public CustomerGetV1 GetCustomer { get; set; }

        public HumanTaskWorkflow(WorkflowDefinitionBuilder<HumanTaskWorkflow, HumanTaskWorkflowInput, HumanTaskWorkflowOutput> builder)
            : base(builder) { }

        public override void BuildDefinition()
        {
            base.BuildDefinition();

            _builder.AddTask(wf => wf.HumanTask, wf => new() { });
            _builder.AddTask(wf => wf.GetCustomer, wf => new() { CustomerId = wf.HumanTask.Output.CustomerId });
        }
    }
}
