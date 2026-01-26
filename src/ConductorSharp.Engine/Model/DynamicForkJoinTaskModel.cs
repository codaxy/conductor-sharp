using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Model
{
    public class DynamicForkJoinInput : ITaskInput<NoOutput>
    {
        public object DynamicTasks { get; set; }

        public object DynamicTasksI { get; set; }
    }

    public class DynamicForkJoinTaskModel : TaskModel<DynamicForkJoinInput, NoOutput>;
}
