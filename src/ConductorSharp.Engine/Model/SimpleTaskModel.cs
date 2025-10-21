using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Model
{
    public abstract class SimpleTaskModel<TInput, TOutput> : TaskModel<TInput, TOutput>, INameable
        where TInput : ITaskInput<TOutput>;
}
