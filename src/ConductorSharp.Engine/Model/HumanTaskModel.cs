using MediatR;
using Newtonsoft.Json;

namespace ConductorSharp.Engine.Model
{
    public class HumanTaskInput<TOutput> : IRequest<TOutput> { }

    public class HumanTaskModel<TOutput> : TaskModel<HumanTaskInput<TOutput>, TOutput> { }
}
