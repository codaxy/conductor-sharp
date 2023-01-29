using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class NestedObjectsInput : WorkflowInput<NestedObjectsOutput> { }

    public class NestedObjectsOutput : WorkflowOutput { }

    public class NestedObjects : Workflow<NestedObjects, NestedObjectsInput, NestedObjectsOutput>
    {
        public NestedObjects(WorkflowDefinitionBuilder<NestedObjects, NestedObjectsInput, NestedObjectsOutput> builder) : base(builder) { }

        public class TestModel
        {
            public int Integer { get; set; }
            public string String { get; set; }
            public object Object { get; set; }
        }

        public Tasks.NestedObjects TaskNestedObjects { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.TaskNestedObjects,
                wf =>
                    new()
                    {
                        NestedObjects = new TestModel
                        {
                            Integer = 1,
                            String = "test",
                            Object = new TestModel
                            {
                                Integer = 1,
                                String = "string",
                                Object = new { NestedInput = "1" }
                            }
                        },
                    }
            );
        }
    }
}
