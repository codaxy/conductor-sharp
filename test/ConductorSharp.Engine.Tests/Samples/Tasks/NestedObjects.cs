using ConductorSharp.Engine.Builders.Metadata;

namespace ConductorSharp.Engine.Tests.Samples.Tasks
{
    public class TaskNestedObjectsInput : ITaskInput<TaskNestedObjectsOutput>
    {
        public object NestedObjects { get; set; }
    }

    public class TaskNestedObjectsOutput { }

    [OriginalName("TEST_task_nested_objects")]
    public class NestedObjects : SimpleTaskModel<TaskNestedObjectsInput, TaskNestedObjectsOutput> { }
}
