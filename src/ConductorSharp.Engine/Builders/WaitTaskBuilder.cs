using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders
{
    public static class WaitTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, WaitTaskModel>> reference,
            Expression<Func<TWorkflow, WaitTaskInput>> input
        ) where TWorkflow : ITypedWorkflow
        {
            var taskBuilder = new WaitTaskBuilder(reference.Body, input.Body, builder.BuildConfiguration);
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    internal class WaitTaskBuilder : BaseTaskBuilder<WaitTaskInput, NoOutput>
    {
        public WaitTaskBuilder(Expression taskExpression, Expression memberExpression, BuildConfiguration buildConfiguration)
            : base(taskExpression, memberExpression, buildConfiguration) { }

        public override WorkflowDefinition.Task[] Build() =>
            new[]
            {
                new WorkflowDefinition.Task
                {
                    Name = $"WAIT_{_taskRefferenceName}",
                    TaskReferenceName = _taskRefferenceName,
                    Type = "WAIT",
                    InputParameters = _inputParameters
                }
            };
    }
}
