using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;

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
            where TInput : ITaskInput<TOutput>
        {
            var taskBuilder = new JsonJqTransformTaskBuilder<TInput, TOutput>(refference.Body, input.Body, builder.BuildConfiguration);
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    public class JsonJqTransformTaskBuilder<TInput, TOutput> : BaseTaskBuilder<TInput, TOutput>
        where TInput : ITaskInput<TOutput>
    {
        public JsonJqTransformTaskBuilder(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration)
            : base(taskExpression, inputExpression, buildConfiguration)
        {
            var queryExpressionValue =
                _inputParameters.GetValue("query_expression") ?? throw new InvalidOperationException("Query expression is a mandatory field");
            _inputParameters.Remove("query_expression");
            _inputParameters.Add("queryExpression", queryExpressionValue);
        }

        public override WorkflowTask[] Build() =>
            [
                new()
                {
                    Name = _taskName,
                    TaskReferenceName = _taskRefferenceName,
                    WorkflowTaskType = WorkflowTaskType.JSON_JQ_TRANSFORM,
                    Type = WorkflowTaskType.JSON_JQ_TRANSFORM.ToString(),
                    InputParameters = _inputParameters.ToObject<IDictionary<string, object>>()
                }
            ];
    }
}
