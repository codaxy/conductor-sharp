using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Handlers;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ConductorSharp.Engine.Builders
{
    public static class CSharpLambdaTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, TWorkflowInput, TWorkflowOutput, TInput, TOutput>(
            this WorkflowDefinitionBuilder<TWorkflow, TWorkflowInput, TWorkflowOutput> builder,
            CSharpLambdaTaskModel<TInput, TOutput> reference
        )
            where TWorkflow : Workflow<TWorkflow, TWorkflowInput, TWorkflowOutput>
            where TWorkflowInput : WorkflowInput<TWorkflowOutput>
            where TWorkflowOutput : WorkflowOutput
            where TInput : IRequest<TOutput>
        {
            //builder.BuildContext.TaskBuilders.Add(new CSharpLambdaTaskBuilder());
            return null;
        }
    }

    internal class CSharpLambdaTaskBuilder<TInput, TOutput> : BaseTaskBuilder<TInput, TOutput> where TInput : IRequest<TOutput>
    {
        private readonly string _lambdaIdentifier;
        private readonly string _lambdaTaskPrefix;

        public CSharpLambdaTaskBuilder(
            Expression taskExpression,
            Expression memberExpression,
            BuildConfiguration buildConfiguration,
            string workflowName
        ) : base(taskExpression, memberExpression, buildConfiguration)
        {
            _lambdaIdentifier = $"{workflowName}.{_taskRefferenceName}";
            _lambdaTaskPrefix = buildConfiguration.LambdaTaskPrefix;
            if (_lambdaTaskPrefix == null)
                throw new NotSupportedException("Can't use lambda tasks without LambdaTaskPrefix set");
        }

        public override WorkflowDefinition.Task[] Build()
        {
            return new[]
            {
                new WorkflowDefinition.Task
                {
                    Name = $"{_lambdaTaskPrefix}.{_taskName}",
                    TaskReferenceName = _taskRefferenceName,
                    InputParameters = new JObject
                    {
                        new JProperty(CSharpLambdaTaskInput.LambdaIdenfitierParamName, _lambdaIdentifier),
                        new JProperty(CSharpLambdaTaskInput.TaskInputParamName, _inputParameters)
                    },
                    Description = new JObject { new JProperty("description", _description) }.ToString(Newtonsoft.Json.Formatting.None),
                    Optional = _additionalParameters.Optional
                }
            };
        }
    }
}
