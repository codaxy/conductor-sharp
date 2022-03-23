using Autofac;
using XgsPn.Handlers;
using XgsPon.WorkflowDefinisions;
using XgsPon.Workflows.Engine.Extensions;

namespace XgsPon.AutofacModules
{
    public class ConductorWorkflowDefinitionsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterWorkflow<TestWorkflow>();
        }
    }
}
