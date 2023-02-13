using ConductorSharp.Engine.Model;
using MediatR;

namespace ConductorSharp.Patterns.Model
{
    public class CSharpLambdaTaskModel<TInput, TOutput> : TaskModel<TInput, TOutput> where TInput : IRequest<TOutput> { }
}
