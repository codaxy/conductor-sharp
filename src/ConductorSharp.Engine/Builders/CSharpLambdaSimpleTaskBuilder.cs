using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
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
            Func<TInput, TOutput> handlerFunc,
            string workflowName,
            AdditionalTaskParameters additionalParameters
        ) : base(taskExpression, inputExpression, additionalParameters)
        {
            _taskName = $"{workflowName}.{_taskRefferenceName}";
            DynamicHandlerBuilder.DefaultBuilder.AddDynamicHandler(handlerFunc, _taskName);
        }
    }
}
