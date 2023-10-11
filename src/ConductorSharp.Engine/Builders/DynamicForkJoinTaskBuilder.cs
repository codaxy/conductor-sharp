using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders
{
    public static class DynamicForkJoinTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, DynamicForkJoinTaskModel>> refference,
            Expression<Func<TWorkflow, DynamicForkJoinInput>> input
        ) where TWorkflow : ITypedWorkflow
        {
            var taskBuilder = new DynamicForkJoinTaskBuilder(refference.Body, input.Body, builder.BuildConfiguration);
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    public class DynamicForkJoinTaskBuilder : BaseTaskBuilder<DynamicForkJoinInput, NoOutput>
    {
        public DynamicForkJoinTaskBuilder(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration)
            : base(taskExpression, inputExpression, buildConfiguration) { }

        public override WorkflowDefinition.Task[] Build()
        {
            var dynamicTaskName = $"FORK_JOIN_DYNAMIC_{_taskRefferenceName}";
            var joinTaskName = $"JOIN_{_taskRefferenceName}";

            return new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = dynamicTaskName,
                    TaskReferenceName = dynamicTaskName,
                    Type = "FORK_JOIN_DYNAMIC",
                    DynamicForkTasksParam = "dynamic_tasks",
                    DynamicForkTasksInputParamName = "dynamic_tasks_i",
                    InputParameters = _inputParameters,
                },
                new WorkflowDefinition.Task
                {
                    Name = joinTaskName,
                    TaskReferenceName = joinTaskName,
                    Type = "JOIN",
                }
            };
        }
    }
}
