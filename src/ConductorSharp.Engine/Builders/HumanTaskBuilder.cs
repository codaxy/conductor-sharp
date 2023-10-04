using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders
{
    public static class HumanTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, HumanTaskModel>> reference,
            Expression<Func<TWorkflow, HumanTaskInput>> input
        ) where TWorkflow : ITypedWorkflow
        {
            var taskBuilder = new HumanTaskBuilder(reference.Body, input.Body, builder.BuildConfiguration);
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    internal class HumanTaskBuilder : BaseTaskBuilder<HumanTaskInput, NoOutput>
    {
        public HumanTaskBuilder(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration)
            : base(taskExpression, inputExpression, buildConfiguration) { }

        public override WorkflowDefinition.Task[] Build() =>
            new[]
            {
                new WorkflowDefinition.Task
                {
                    Name = $"HUMAN_{_taskRefferenceName}",
                    TaskReferenceName = _taskRefferenceName,
                    Type = "HUMAN",
                    InputParameters = _inputParameters,
                }
            };
    }
}
