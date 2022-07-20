using MediatR;

namespace ConductorSharp.Engine.Model
{
    public abstract class LambdaOutputModel<O>
    {
        public O Result { get; set; }
    }

    public abstract class LambdaTaskModel<I, O> where I : IRequest<O>
    {
        public I Input { get; set; }
        public LambdaOutputModel<O> Output { get; set; }
    }
}
