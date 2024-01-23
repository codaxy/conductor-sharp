using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Builders
{
    public static class PassThroughTaskExtensions
    {
        public static void AddTasks<TWorkflow>(this ITaskSequenceBuilder<TWorkflow> builder, params WorkflowTask[] taskDefinitions)
            where TWorkflow : ITypedWorkflow => builder.AddTaskBuilderToSequence(new PassThroughTaskBuilder(taskDefinitions));
    }

    public class PassThroughTaskBuilder(WorkflowTask[] tasks) : ITaskBuilder
    {
        private readonly WorkflowTask[] _tasks = tasks;

        public WorkflowTask[] Build() => _tasks;
    }
}
