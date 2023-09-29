using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class ListInitializationWorkflowInput : WorkflowInput<ListInitializationWorkflowOutput> { }

    public class ListInitializationWorkflowOutput : WorkflowOutput { }

    public class ListInitializationWorkflow : Workflow<ListInitializationWorkflow, ListInitializationWorkflowInput, ListInitializationWorkflowOutput>
    {
        public ListTask ListTask { get; set; }

        public ListInitializationWorkflow(
            WorkflowDefinitionBuilder<ListInitializationWorkflow, ListInitializationWorkflowInput, ListInitializationWorkflowOutput> builder
        ) : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.ListTask,
                wf =>
                    new()
                    {
                        List = new()
                        {
                            1,
                            "str",
                            new { TestProp = "testProp" },
                            new NestedObjects.TestModel { Integer = 1 }
                        }
                    }
            );
        }
    }
}
