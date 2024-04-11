using ConductorSharp.Client.Generated;

namespace ConductorSharp.Engine.Interface
{
    public interface ITypedWorkflow : INameable
    {
        WorkflowDef GetDefinition();
    }
}
