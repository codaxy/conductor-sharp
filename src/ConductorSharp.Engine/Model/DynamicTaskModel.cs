using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Model
{
    public class DynamicTaskInput<TInput, TOutput> : ITaskInput<TOutput>
    {
        public TInput TaskInput { get; set; }
        public string TaskToExecute { get; set; }
    }

    public class DynamicTaskModel<TInput, TOutput> : TaskModel<DynamicTaskInput<TInput, TOutput>, TOutput>;
}
