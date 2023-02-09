using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Exceptions;
using ConductorSharp.Engine.Handlers;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ConductorSharp.Engine.Builders
{
    public static class CSharpLambdaTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, TWorkflowInput, TWorkflowOutput, TInput, TOutput>(
            this WorkflowDefinitionBuilder<TWorkflow, TWorkflowInput, TWorkflowOutput> builder,
            Expression<Func<TWorkflow, CSharpLambdaTaskModel<TInput, TOutput>>> task,
            Expression<Func<TWorkflow, TInput>> input,
            Func<TInput, TOutput> lambda
        )
            where TWorkflow : Workflow<TWorkflow, TWorkflowInput, TWorkflowOutput>
            where TWorkflowInput : WorkflowInput<TWorkflowOutput>
            where TWorkflowOutput : WorkflowOutput
            where TInput : IRequest<TOutput>
        {
            var lambdaTaskNamePrefix = (string)
                builder.ConfigurationProperties.First(prop => prop.Key == CSharpLambdaTaskHandler.LambdaTaskNameConfigurationProperty).Value;
            var taskBuilder = new CSharpLambdaTaskBuilder<TInput, TOutput>(task.Body, input.Body, builder.BuildConfiguration, lambdaTaskNamePrefix);
            builder.WorkflowBuildRegistry.Register<TWorkflow>(
                taskBuilder.LambdaIdentifer,
                new CSharpLambdaHandler(taskBuilder.LambdaIdentifer, typeof(TInput), lambda)
            );
            builder.BuildContext.TaskBuilders.Add(taskBuilder);
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
            string lambdaTaskNamePrefix
        ) : base(taskExpression, memberExpression, buildConfiguration)
        {
            LambdaIdentifer = $"{0}.{_taskRefferenceName}";
            _lambdaTaskNamePrefix = lambdaTaskNamePrefix;
        }

        public override WorkflowDefinition.Task[] Build()
        {
            return new[]
            {
                new WorkflowDefinition.Task
                {
                    Name = $"{_lambdaTaskNamePrefix}.{CSharpLambdaTaskHandler.TaskName}",
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
