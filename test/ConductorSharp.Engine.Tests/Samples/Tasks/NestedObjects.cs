using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Tasks
{
    public class TaskNestedObjectsInput : IRequest<TaskNestedObjectsOutput>
    {
        public dynamic NestedObjects { get; set; }
    }

    public class TaskNestedObjectsOutput { }

    [OriginalName("TEST_task_nested_objects")]
    public class NestedObjects : SimpleTaskModel<TaskNestedObjectsInput, TaskNestedObjectsOutput> { }
}
