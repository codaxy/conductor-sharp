using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Model
{
    public class JsonJqTransformTaskModel<TInput, TOutput> : TaskModel<TInput, TOutput>
        where TInput : ITaskInput<TOutput>;
}
