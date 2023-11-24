using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ConductorSharp.Engine.Builders
{
    public static class SubWorkflowTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, TInput, TOutput>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, SubWorkflowTaskModel<TInput, TOutput>>> referrence,
            Expression<Func<TWorkflow, TInput>> input
        )
            where TWorkflow : ITypedWorkflow
            where TInput : IRequest<TOutput>
        {
            var taskBuilder = new SubWorkflowTaskBuilder<TInput, TOutput>(referrence.Body, input.Body, builder.BuildConfiguration);
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    public class SubWorkflowTaskBuilder<TInput, TOutput> : BaseTaskBuilder<TInput, TOutput> where TInput : IRequest<TOutput>
    {
        private readonly int _version;

        public SubWorkflowTaskBuilder(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration)
            : base(taskExpression, inputExpression, buildConfiguration) => _version = GetVersion(taskExpression);

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
