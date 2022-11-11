using ConductorSharp.Patterns.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class PatternTasksInput : WorkflowInput<PatternTasksOutput>
    {
        public int Seconds { get; set; }
        public string WorkflowId { get; set; }
    }

    public class PatternTasksOutput : WorkflowOutput
    {
        public int WaitTime { get; set; }
    }

    public class PatternTasks : Workflow<PatternTasksInput, PatternTasksOutput>
    {
        public WaitSeconds WaitSeconds { get; set; }
        public ReadWorkflowTasks ReadWorkflowTasks { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<PatternTasks, PatternTasksInput, PatternTasksOutput>();

            builder.AddTask(wf => wf.ReadWorkflowTasks, wf => new() { TaskNames = "task1,task2", WorkflowId = wf.WorkflowInput.WorkflowId });

            builder.AddTask(wf => wf.WaitSeconds, wf => new() { Seconds = wf.WorkflowInput.Seconds });

            return builder.Build(opts => opts.Version = 1);
        }
    }
}
