using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using System;
using System.Collections.Generic;
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

    internal class HumanTaskBuilder(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration) : BaseTaskBuilder<HumanTaskInput, NoOutput>(taskExpression, inputExpression, buildConfiguration)
    {
        public override WorkflowTask[] Build() =>

            [
                new()
                {
                    Name = $"HUMAN_{_taskRefferenceName}",
                    TaskReferenceName = _taskRefferenceName,
                    Type = "HUMAN",
                    InputParameters = _inputParameters.ToObject<IDictionary<string,object>>(),
                }
            ];
    }
}
