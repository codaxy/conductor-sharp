using Autofac;
using ConductorSharp.Client.Model.Common;

namespace ConductorSharp.Engine.Interface
{
    public interface ITypedWorkflow : INameable
    {
        WorkflowDefinition GetDefinition();
        void OnRegistration(ContainerBuilder containerBuilder);
        void OnGetDefinition(WorkflowDefinition workflowDefinition);
        void OnResolve(IComponentContext componentContext);
    }
}
