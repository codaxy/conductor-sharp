using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using ConductorSharp.Patterns.Exceptions;
using ConductorSharp.Patterns.Model;
using ConductorSharp.Patterns.Tasks;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
            var prefixString = builder.ConfigurationProperties.FirstOrDefault(
                prop => prop.Key == CSharpLambdaTask.LambdaTaskNameConfigurationProperty
            )?.Value as string ?? throw new LambdaTasksNotEnabledException();

            var lambdaTaskNamePrefix = TaskNameBuilder.MakeTaskNamePrefix(prefixString);

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

        public override WorkflowTask[] Build()
        {
            return 
            [
                new()
                {
                    Name = $"{_lambdaTaskNamePrefix}{CSharpLambdaTask.TaskName}",
                    TaskReferenceName = _taskRefferenceName,
                    InputParameters = new JObject
                    {
                        new JProperty(CSharpLambdaTaskInput.LambdaIdenfitierParamName, LambdaIdentifer),
                        new JProperty(CSharpLambdaTaskInput.TaskInputParamName, _inputParameters)
                    }.ToObject<IDictionary<string,object>>(),
                    Optional = _additionalParameters.Optional
                }
            ];
        }
    }
}
