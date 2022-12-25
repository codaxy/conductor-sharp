using Autofac;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Util.Builders;

namespace ConductorSharp.Engine.Interface
{
    public delegate void LoadWorflow(ContainerBuilder containerBuilder);
    public delegate void ResolveWorkflow(IComponentContext componntContext);
    public delegate void GetWorkflowDefinition(WorkflowDefinition workflowDefinition);

    public interface ITypedWorkflow : INameable
    {
        WorkflowDefinition GetDefinition();
    }
}
