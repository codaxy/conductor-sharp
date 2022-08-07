using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using MediatR;
using System;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders
{
    internal class CSharpLambdaSimpleTaskBuilder<TInput, TOutput> : SimpleTaskBuilder<TInput, TOutput> where TInput : IRequest<TOutput>
    {
        public CSharpLambdaSimpleTaskBuilder(
            Expression taskExpression,
            Expression inputExpression,
            string taskName,
            AdditionalTaskParameters additionalParameters
        ) : base(taskExpression, inputExpression, additionalParameters) => _taskName = taskName;
    }
}
