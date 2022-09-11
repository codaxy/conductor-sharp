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
        public dynamic InputOne { get; set; }
    }

    public class ScaffoldedOneOutput : WorkflowOutput
    {
        public dynamic OutputOne { get; set; }
    }

    [OriginalName("SCAFFOLDED_one")]
    public class ScaffoldedOne : SubWorkflowTaskModel<ScaffoldedOneInput, ScaffoldedOneOutput> { }
    #endregion
    #region s_two
    public class ScaffoldedTwoInput : WorkflowInput<ScaffoldedTwoOutput>
    {
        public dynamic Name { get; set; }
    }

    public class ScaffoldedTwoOutput : WorkflowOutput
    {
        public dynamic OutputName { get; set; }
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
    public class ScaffoldedWorkflows : Workflow<ScaffoldedWorkflowsInput, ScaffoldedWorkflowsOutput>
    {
        public ScaffoldedOne ScaffOne { get; set; }
        public ScaffoldedTwo ScaffTwo { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<ScaffoldedWorkflows>();

            builder.AddTask(wf => wf.ScaffOne, wf => new() { InputOne = wf.WorkflowInput.CustomerId });

            builder.AddTask(wf => wf.ScaffTwo, wf => new() { Name = wf.ScaffOne.Output.OutputOne });

            return builder.Build(opts => opts.Version = 1);
        }
    }
}
