using ConductorSharp.Client.Model.Common;

namespace ConductorSharp.Engine.Interface
{
    public interface ITaskBuilder
    {
        //void Build();
        WorkflowDefinition.Task[] Build();
    }
}
