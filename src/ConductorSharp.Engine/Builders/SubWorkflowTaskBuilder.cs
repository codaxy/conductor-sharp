using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;

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
            where TInput : ITaskInput<TOutput>
        {
            var taskBuilder = new SubWorkflowTaskBuilder<TInput, TOutput>(referrence.Body, input.Body, builder.BuildConfiguration);
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    public class SubWorkflowTaskBuilder<TInput, TOutput>(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration)
        : BaseTaskBuilder<TInput, TOutput>(taskExpression, inputExpression, buildConfiguration)
        where TInput : ITaskInput<TOutput>
    {
        private readonly int _version = GetVersion(taskExpression);

        public override WorkflowTask[] Build() =>
            [
                new()
                {
                    Name = _taskName,
                    TaskReferenceName = _taskRefferenceName,
                    WorkflowTaskType = WorkflowTaskType.SUB_WORKFLOW,
                    Type = WorkflowTaskType.SUB_WORKFLOW.ToString(),
                    InputParameters = _inputParameters.ToObject<IDictionary<string, object>>(),
                    SubWorkflowParam = new SubWorkflowParams { Name = _taskName, Version = _version },
                    Optional = _additionalParameters?.Optional == true
                }
            ];

        private static int GetVersion(Expression taskExpression)
        {
            var type = ExpressionUtil.ParseToType(taskExpression);
            return type.GetCustomAttribute<VersionAttribute>()?.Version ?? 1;
        }
    }
}
