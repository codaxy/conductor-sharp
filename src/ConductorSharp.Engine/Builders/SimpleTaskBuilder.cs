using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders
{
    public static class SimpleTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, F, G>(
            this WorkflowDefinitionBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, SimpleTaskModel<F, G>>> refference,
            Expression<Func<TWorkflow, F>> input,
            AdditionalTaskParameters additionalParameters = null
        )
            where TWorkflow : ITypedWorkflow
            where F : IRequest<G>
        {
            var taskBuilder = new SimpleTaskBuilder<F, G>(refference.Body, input.Body, additionalParameters);
            builder.Context.TaskBuilders.Add(taskBuilder);
            return taskBuilder;
        }
    }

    public class SimpleTaskBuilder<A, B> : BaseTaskBuilder<A, B> where A : IRequest<B>
    {
        public SimpleTaskBuilder(Expression taskExpression, Expression inputExpression, AdditionalTaskParameters additionalParameters)
            : base(taskExpression, inputExpression)
        {
            _additionalParameters = additionalParameters ?? _additionalParameters;
        }

        public override WorkflowDefinition.Task[] Build() =>
            new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = _taskName,
                    TaskReferenceName = _taskRefferenceName,
                    Type = "SIMPLE",
                    InputParameters = _inputParameters,
                    Description = new JObject { new JProperty("description", _description) }.ToString(Newtonsoft.Json.Formatting.None),
                    Optional = _additionalParameters?.Optional == true,
                }
            };
    }
}
