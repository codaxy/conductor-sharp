using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class NestedObjectsInput : WorkflowInput<NestedObjectsOutput> { }

    public class NestedObjectsOutput : WorkflowOutput { }

    public class NestedObjects : Workflow<NestedObjectsInput, NestedObjectsOutput>
    {
        public class TestModel
        {
            public int Integer { get; set; }
            public string String { get; set; }
            public object Object { get; set; }
        }

        public TaskNestedObjects TaskNestedObjects { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<NestedObjects>();

            builder.AddTask(
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

            return builder.Build(opts => opts.Version = 1);
        }
    }
}
