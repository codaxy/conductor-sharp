using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Model
{
    public abstract class LambdaOutputModel<O>
    {
        public O Result { get; set; }
    }

    public abstract class LambdaTaskModel<TInput, TOutput>
        where TInput : ITaskInput<TOutput>
    {
        public TInput Input { get; set; }
        public LambdaOutputModel<TOutput> Output { get; set; }
    }
}
