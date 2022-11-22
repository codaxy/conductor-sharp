using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders.Configurable
{
    public static class SimpleTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, TInput, TOutput, F, G>(
            this WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> builder,
            Expression<Func<TWorkflow, SimpleTaskModel<F, G>>> refference,
            Expression<Func<TWorkflow, F>> input,
            AdditionalTaskParameters additionalParameters = null
        )
            where TWorkflow : Workflow<TWorkflow, TInput, TOutput>
            where TInput : WorkflowInput<TOutput>
            where TOutput : WorkflowOutput
            where F : IRequest<G>
        {
            var taskBuilder = new SimpleTaskBuilder<F, G>(refference.Body, input.Body, additionalParameters, builder.BuildConfiguration);
            builder.BuildContext.TaskBuilders.Add(taskBuilder);
            return taskBuilder;
        }
    }

    public class SimpleTaskBuilder<A, B> : BaseTaskBuilder<A, B> where A : IRequest<B>
    {
        public SimpleTaskBuilder(
            Expression taskExpression,
            Expression inputExpression,
            AdditionalTaskParameters additionalParameters,
            BuildConfiguration buildConfiguration
        ) : base(taskExpression, inputExpression, buildConfiguration)
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
