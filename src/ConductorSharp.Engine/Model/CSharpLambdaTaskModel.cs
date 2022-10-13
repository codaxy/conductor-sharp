using ConductorSharp.Engine.Handlers;
using ConductorSharp.Engine.Util;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Model
{
    [OriginalName(CSharpLambdaTaskHandler.TaskName)]
    public sealed class CSharpLambdaTaskModel<TInput, TOutput> : SimpleTaskModel<TInput, TOutput> where TInput : IRequest<TOutput> { }
}
