using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders
{
    public static class JsonJqTransformTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, TInput, TOutput>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, JsonJqTransformTaskModel<TInput, TOutput>>> refference,
            Expression<Func<TWorkflow, TInput>> input
        )
            where TWorkflow : ITypedWorkflow
            where TInput : IRequest<TOutput>
        {
            var taskBuilder = new JsonJqTransformTaskBuilder<TInput, TOutput>(refference.Body, input.Body, builder.BuildConfiguration);
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    public class JsonJqTransformTaskBuilder<A, B> : BaseTaskBuilder<A, B> where A : IRequest<B>
    {
        public JsonJqTransformTaskBuilder(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration)
            : base(taskExpression, inputExpression, buildConfiguration)
        {
            var queryExpressionValue = _inputParameters.GetValue("query_expression") ?? throw new InvalidOperationException("Query expression is a mandatory field");
            _inputParameters.Remove("query_expression");
            _inputParameters.Add("queryExpression", queryExpressionValue);
        }

        public override WorkflowTask[] Build() =>

            [
                new()
                {
                    Name = _taskName,
                    TaskReferenceName = _taskRefferenceName,
                    Type = "JSON_JQ_TRANSFORM",
                    InputParameters = _inputParameters.ToObject<IDictionary<string,object>>()
                }
            ];
    }
}
