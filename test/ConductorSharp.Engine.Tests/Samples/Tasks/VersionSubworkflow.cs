using ConductorSharp.Engine.Builders.Metadata;

namespace ConductorSharp.Engine.Tests.Samples.Tasks
{
    public class VersionSubworkflowInput : ITaskInput<VersionSubworkflowOutput> { }

    public class VersionSubworkflowOutput;

    [OriginalName("TEST_subworkflow")]
    [Version(3)]
    public class VersionSubworkflow : SubWorkflowTaskModel<VersionSubworkflowInput, VersionSubworkflowOutput>;
}
