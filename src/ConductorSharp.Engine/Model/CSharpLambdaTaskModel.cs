using MediatR;

namespace ConductorSharp.Engine.Model
{
    public class CSharpLambdaTaskModel<TInput, TOutput> : TaskModel<TInput, TOutput> where TInput : IRequest<TOutput> { }
}
