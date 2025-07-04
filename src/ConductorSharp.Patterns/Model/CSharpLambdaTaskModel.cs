using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;

namespace ConductorSharp.Patterns.Model
{
    public class CSharpLambdaTaskModel<TInput, TOutput> : TaskModel<TInput, TOutput>
        where TInput : ITaskInput<TOutput>;
}
