using ConductorSharp.Client.Model.Common;

namespace ConductorSharp.Engine.Interface
{

    public interface ITypedWorkflow : INameable
    {
        WorkflowDefinition GetDefinition();
    }
}