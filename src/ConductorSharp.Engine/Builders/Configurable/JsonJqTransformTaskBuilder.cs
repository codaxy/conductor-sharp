using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using MediatR;
using System;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders.Configurable
{
    public static class JsonJqTransformTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, TInput, TOutput, F, G>(
            this WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> builder,
            Expression<Func<TWorkflow, JsonJqTransformTaskModel<F, G>>> refference,
            Expression<Func<TWorkflow, F>> input
        )
            where TWorkflow : Workflow<TWorkflow, TInput, TOutput>
            where TInput : WorkflowInput<TOutput>
            where TOutput : WorkflowOutput
            where F : IRequest<G>
        {
            var taskBuilder = new JsonJqTransformTaskBuilder<F, G>(refference.Body, input.Body, builder.BuildConfiguration);
            builder.BuildContext.TaskBuilders.Add(taskBuilder);
            return taskBuilder;
        }
    }

    public class JsonJqTransformTaskBuilder<A, B> : BaseTaskBuilder<A, B> where A : IRequest<B>
    {
        public JsonJqTransformTaskBuilder(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration)
            : base(taskExpression, inputExpression, buildConfiguration)
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
