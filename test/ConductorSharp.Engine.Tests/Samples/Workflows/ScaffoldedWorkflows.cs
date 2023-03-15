using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    #region s_one
    public class ScaffoldedOneInput : WorkflowInput<ScaffoldedOneOutput>
    {
        public object InputOne { get; set; }
    }

    public class ScaffoldedOneOutput : WorkflowOutput
    {
        public object OutputOne { get; set; }
    }

    [OriginalName("SCAFFOLDED_one")]
    public class ScaffoldedOne : SubWorkflowTaskModel<ScaffoldedOneInput, ScaffoldedOneOutput> { }
    #endregion
    #region s_two
    public class ScaffoldedTwoInput : WorkflowInput<ScaffoldedTwoOutput>
    {
        public object Name { get; set; }
    }

    public class ScaffoldedTwoOutput : WorkflowOutput
    {
        public object OutputName { get; set; }
    }

    [OriginalName("SCAFFOLDED_two")]
    public class ScaffoldedTwo : SubWorkflowTaskModel<ScaffoldedTwoInput, ScaffoldedTwoOutput> { }
    #endregion
    public class ScaffoldedWorkflowsInput : WorkflowInput<ScaffoldedWorkflowsOutput>
    {
        public int CustomerId { get; set; }
    }

    public class ScaffoldedWorkflowsOutput : WorkflowOutput { }

    [OriginalName("SCAFFOLDED_workflows")]
    public class ScaffoldedWorkflows : Workflow<ScaffoldedWorkflows, ScaffoldedWorkflowsInput, ScaffoldedWorkflowsOutput>
    {
        public ScaffoldedWorkflows(WorkflowDefinitionBuilder<ScaffoldedWorkflows, ScaffoldedWorkflowsInput, ScaffoldedWorkflowsOutput> builder)
            : base(builder) { }

        public ScaffoldedOne ScaffOne { get; set; }
        public ScaffoldedTwo ScaffTwo { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.ScaffOne, wf => new() { InputOne = wf.WorkflowInput.CustomerId });

            _builder.AddTask(wf => wf.ScaffTwo, wf => new() { Name = wf.ScaffOne.Output.OutputOne });
        }
    }
}
