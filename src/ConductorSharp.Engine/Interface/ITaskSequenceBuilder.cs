using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using System.Collections.Generic;

namespace ConductorSharp.Engine.Interface
{
    public interface ITaskSequenceBuilder<TWorkflow> where TWorkflow : ITypedWorkflow
    {
        BuildContext BuildContext { get; }
        BuildConfiguration BuildConfiguration { get; }
        WorkflowBuildItemRegistry WorkflowBuildRegistry { get; }
        IEnumerable<ConfigurationProperty> ConfigurationProperties { get; }
        void AddTaskBuilderToSequence(ITaskBuilder builder);
    }
}
