using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class ArrayInput : WorkflowInput<ArrayOutput>
    {
        public string TestValue { get; set; }
    }

    public class ArrayOutput : WorkflowOutput { }

    public class Arrays : Workflow<Arrays, ArrayInput, ArrayOutput>
    {
        public Arrays(WorkflowDefinitionBuilder<Arrays, ArrayInput, ArrayOutput> builder) : base(builder) { }

        public ArrayTask ArrayTask1 { get; set; }
        public ArrayTask ArrayTask2 { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.ArrayTask1,
                wf =>
                    new()
                    {
                        Integers = new[] { 1, 2, 3 },
                        TestModelList = new List<ArrayTaskInput.TestModel>
                        {
                            new ArrayTaskInput.TestModel { String = wf.Input.TestValue },
                            new ArrayTaskInput.TestModel { String = "List2" }
                        },
                        Models = new[]
                        {
                            new ArrayTaskInput.TestModel { String = "Test1" },
                            new ArrayTaskInput.TestModel { String = "Test2" }
                        },
                        Objects = new dynamic[] { new { AnonymousObjProp = "Prop" }, new { Test = "Prop" } }
                    }
            );

            _builder.AddTask(
                wf => wf.ArrayTask2,
                wf =>
                    new()
                    {
                        Integers = new int[] { },
                        Models = new ArrayTaskInput.TestModel[] { },
                        Objects = new dynamic[] { }
                    }
            );
        }
    }
}
