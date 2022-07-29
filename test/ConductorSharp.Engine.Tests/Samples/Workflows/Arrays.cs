using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class ArrayInput : WorkflowInput<ArrayOutput> { }

    public class ArrayOutput : WorkflowOutput { }

    public class Arrays : Workflow<ArrayInput, ArrayOutput>
    {
        public ArrayTask ArrayTask { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<Arrays>();

            builder.AddTask(
                wf => wf.ArrayTask,
                wf =>
                    new()
                    {
                        Integers = new[] { 1, 2, 3 },
                        Models = new[]
                        {
                            new ArrayTaskInput.TestModel { String = "Test1" },
                            new ArrayTaskInput.TestModel { String = "Test2" }
                        },
                        Objects = new dynamic[] { new { AnonymousObjProp = "Prop" }, new { Test = "Prop" } }
                    }
            );

            builder.AddTask(
                wf => wf.ArrayTask,
                wf =>
                    new()
                    {
                        Integers = new int[] { },
                        Models = new ArrayTaskInput.TestModel[] { },
                        Objects = new dynamic[] { }
                    }
            );

            return builder.Build(opts => opts.Version = 1);
        }
    }
}
