using ConductorSharp.Client.Generated;

namespace ConductorSharp.Engine.Interface
{
    public interface ITaskBuilder
    {
        WorkflowTask[] Build();
    }
}
