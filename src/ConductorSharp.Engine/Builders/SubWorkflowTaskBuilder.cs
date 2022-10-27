using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ConductorSharp.Engine.Builders
{
    public static class SubWorkflowTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, F, G>(
            this WorkflowDefinitionBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, SubWorkflowTaskModel<F, G>>> referrence,
            Expression<Func<TWorkflow, F>> input
        )
            where TWorkflow : ITypedWorkflow
            where F : IRequest<G>
        {
            var taskBuilder = new SubWorkflowTaskBuilder<F, G>(referrence.Body, input.Body);
            builder.Context.TaskBuilders.Add(taskBuilder);
            return taskBuilder;
        }
    }

    public class SubWorkflowTaskBuilder<TInput, TOutput> : BaseTaskBuilder<TInput, TOutput> where TInput : IRequest<TOutput>
    {
        private readonly int _version;

        public SubWorkflowTaskBuilder(Expression taskExpression, Expression inputExpression) : base(taskExpression, inputExpression) =>
            _version = GetVersion(taskExpression);

        public override WorkflowDefinition.Task[] Build() =>
            new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = _taskName,
                    TaskReferenceName = _taskRefferenceName,
                    Type = "SUB_WORKFLOW",
                    InputParameters = _inputParameters,
                    SubWorkflowParam = new WorkflowDefinition.SubWorkflowParam { Name = _taskName, Version = _version },
                    Description = new JObject { new JProperty("description", _description) }.ToString(Newtonsoft.Json.Formatting.None),
                    Optional = _additionalParameters?.Optional == true
                }
            };

        private int GetVersion(Expression taskExpression)
        {
            var type = ExpressionUtil.ParseToType(taskExpression);
            return type.GetCustomAttribute<VersionAttribute>()?.Version ?? 1;
        }
    }
}
