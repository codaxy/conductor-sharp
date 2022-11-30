using Autofac;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Util.Builders;

namespace ConductorSharp.Engine.Interface
{
    public interface ITypedWorkflow : INameable
    {
        WorkflowDefinition GetDefinition();
        void OnGetDefinition(WorkflowDefinition workflowDefinition);
        void BeforeGetDefinition(IComponentContext componentContext, BuildConfiguration buildConfiguration);
    }
}
