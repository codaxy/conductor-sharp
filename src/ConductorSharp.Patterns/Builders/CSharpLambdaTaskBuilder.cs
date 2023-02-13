using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using ConductorSharp.Patterns.Model;
using ConductorSharp.Patterns.Tasks;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace ConductorSharp.Patterns.Builders
{
    public static class CSharpLambdaTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, TInput, TOutput>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, CSharpLambdaTaskModel<TInput, TOutput>>> task,
            Expression<Func<TWorkflow, TInput>> input,
            Func<TInput, TOutput> lambda
        )
            where TWorkflow : ITypedWorkflow
            where TInput : IRequest<TOutput>
        {
            var lambdaTaskNamePrefix = (string)
                builder.ConfigurationProperties.First(prop => prop.Key == CSharpLambdaTask.LambdaTaskNameConfigurationProperty).Value;
            var taskBuilder = new CSharpLambdaTaskBuilder<TInput, TOutput>(
                task.Body,
                input.Body,
                builder.BuildConfiguration,
                lambdaTaskNamePrefix,
                builder.BuildContext.WorkflowName
            );
            builder.WorkflowBuildRegistry.Register<TWorkflow>(
                taskBuilder.LambdaIdentifer,
                new CSharpLambdaHandler(taskBuilder.LambdaIdentifer, typeof(TInput), lambda)
            );
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    internal class CSharpLambdaTaskBuilder<TInput, TOutput> : BaseTaskBuilder<TInput, TOutput> where TInput : IRequest<TOutput>
    {
        public const string LambdaIdStorageKey = "ConductorSharp.Engine.CSharpLambdaTaskBuilder.LambdaId";

        public string LambdaIdentifer { get; }

        private readonly string _lambdaTaskNamePrefix;

        public CSharpLambdaTaskBuilder(
            Expression taskExpression,
            Expression memberExpression,
            BuildConfiguration buildConfiguration,
            string lambdaTaskNamePrefix,
            string workflowName
        ) : base(taskExpression, memberExpression, buildConfiguration)
        {
            LambdaIdentifer = $"{workflowName}.{_taskRefferenceName}";
            _lambdaTaskNamePrefix = lambdaTaskNamePrefix;
        }

        public override WorkflowDefinition.Task[] Build()
        {
            return new[]
            {
                new WorkflowDefinition.Task
                {
                    Name = $"{_lambdaTaskNamePrefix}.{CSharpLambdaTask.TaskName}",
                    TaskReferenceName = _taskRefferenceName,
                    InputParameters = new JObject
                    {
                        new JProperty(CSharpLambdaTaskInput.LambdaIdenfitierParamName, LambdaIdentifer),
                        new JProperty(CSharpLambdaTaskInput.TaskInputParamName, _inputParameters)
                    },
                    Description = new JObject { new JProperty("description", _description) }.ToString(Newtonsoft.Json.Formatting.None),
                    Optional = _additionalParameters.Optional
                }
            };
        }
    }
}
