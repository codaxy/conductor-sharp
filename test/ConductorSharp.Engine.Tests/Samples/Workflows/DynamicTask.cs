using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class DynamicTaskInput : WorkflowInput<DynamicTaskOutput>
    {
        public string TaskName { get; set; }
        public int WorkflowVersion { get; set; }
        public int Count { get; set; }
        public bool ShouldUseNext { get; set; }
    }

    public class DynamicTaskOutput : WorkflowOutput { }

    public class ExpectedDynamicOutput
    {
        public string Name { get; set; }
    }

    public class MandatoryDynamicInput
    {
        public int Count { get; set; }
        public bool ShouldUseNext { get; set; }
    }

    [OriginalName("TEST_dynamic_task")]
    public class DynamicTask : Workflow<DynamicTaskInput, DynamicTaskOutput>
    {
        public DynamicTaskModel<MandatoryDynamicInput, ExpectedDynamicOutput> DynamicHandler { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<DynamicTask>();

            builder.AddTask(
                a => a.DynamicHandler,
                b =>
                    new()
                    {
                        TaskToExecute = b.WorkflowInput.TaskName,
                        TaskInput = new() { Count = b.WorkflowInput.Count, ShouldUseNext = b.WorkflowInput.ShouldUseNext }
                    }
            );

            return builder.Build(opts => opts.Version = 1);
        }
    }
}
