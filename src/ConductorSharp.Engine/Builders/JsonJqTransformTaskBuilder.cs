using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using MediatR;
using System;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders
{
    public static class JsonJqTransformTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, F, G>(
            this WorkflowDefinitionBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, JsonJqTransformTaskModel<F, G>>> refference,
            Expression<Func<TWorkflow, F>> input
        )
            where TWorkflow : ITypedWorkflow
            where F : IRequest<G>
        {
            var taskBuilder = new JsonJqTransformTaskBuilder<F, G>(refference.Body, input.Body);
            builder.Context.TaskBuilders.Add(taskBuilder);
            return taskBuilder;
        }
    }

    public class JsonJqTransformTaskBuilder<A, B> : BaseTaskBuilder<A, B> where A : IRequest<B>
    {
        public JsonJqTransformTaskBuilder(Expression taskExpression, Expression inputExpression) : base(taskExpression, inputExpression)
        {
            var queryExpressionValue = _inputParameters.GetValue("query_expression");

            if (queryExpressionValue == null)
                throw new InvalidOperationException("Query expression is a mandatory field");

            _inputParameters.Remove("query_expression");
            _inputParameters.Add("queryExpression", queryExpressionValue);
        }

        public override WorkflowDefinition.Task[] Build() =>
            new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = _taskName,
                    TaskReferenceName = _taskRefferenceName,
                    Type = "JSON_JQ_TRANSFORM",
                    InputParameters = _inputParameters
                }
            };
    }
}
