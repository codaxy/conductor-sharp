using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine
{
    public abstract class TaskRequestHandler<TInput, TOutput> : SimpleTaskModel<TInput, TOutput>, ITaskRequestHandler<TInput, TOutput>
        where TInput : IRequest<TOutput>
    {
        public abstract Task<TOutput> Handle(TInput request, CancellationToken cancellationToken);
    }
}
