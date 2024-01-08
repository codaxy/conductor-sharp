using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using System;
using System.Collections.Generic;
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

    public class DynamicForkJoinTaskBuilder(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration) : BaseTaskBuilder<DynamicForkJoinInput, NoOutput>(taskExpression, inputExpression, buildConfiguration)
    {
        public override WorkflowTask[] Build()
        {
            var dynamicTaskName = $"FORK_JOIN_DYNAMIC_{_taskRefferenceName}";
            var joinTaskName = $"JOIN_{_taskRefferenceName}";

            return
            [
                new() {
                    Name = dynamicTaskName,
                    TaskReferenceName = dynamicTaskName,
                    WorkflowTaskType = WorkflowTaskType.FORK_JOIN_DYNAMIC,
                    DynamicForkTasksParam = "dynamic_tasks",
                    DynamicForkTasksInputParamName = "dynamic_tasks_i",
                    InputParameters = _inputParameters.ToObject<IDictionary<string,object>>(),
                },
                new() {
                    Name = joinTaskName,
                    TaskReferenceName = joinTaskName,
                    Type = "JOIN",
                }
            ];
        }
    }
}
