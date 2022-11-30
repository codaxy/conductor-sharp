using Autofac;
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
    public static class LambdaTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, TInput, TOutput, F, G>(
            this WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> builder,
            Expression<Func<TWorkflow, LambdaTaskModel<F, G>>> referrence,
            Expression<Func<TWorkflow, F>> input,
            string script
        )
            where TWorkflow : Workflow<TWorkflow, TInput, TOutput>
            where TInput : WorkflowInput<TOutput>
            where TOutput : WorkflowOutput
            where F : IRequest<G>
        {
            var taskBuilder = new LambdaTaskBuilder<F, G>(script, referrence.Body, input.Body, builder.BuildConfiguration);

            builder.BuildContext.TaskBuilders.Add(taskBuilder);
            return taskBuilder;
        }
    }

    public class LambdaTaskBuilder<A, B> : BaseTaskBuilder<A, B> where A : IRequest<B>
    {
        private readonly string _script;

        public LambdaTaskBuilder(string script, Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration)
            : base(taskExpression, inputExpression, buildConfiguration) => _script = script;

        public override WorkflowDefinition.Task[] Build()
        {
            _inputParameters.Add(new JProperty("scriptExpression", _script));
            return new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = $"LAMBDA_{_taskRefferenceName}",
                    TaskReferenceName = _taskRefferenceName,
                    Type = "LAMBDA",
                    Description = new JObject { new JProperty("description", _description) }.ToString(Newtonsoft.Json.Formatting.None),
                    InputParameters = _inputParameters
                }
            };
        }
    }
}
