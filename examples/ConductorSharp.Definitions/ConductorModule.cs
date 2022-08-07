using Autofac;
using ConductorSharp.Definitions.Workflows;
using ConductorSharp.Engine.Extensions;

namespace ConductorSharp.Definitions
{
    public class ConductorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterWorkflow<SendCustomerNotification>();
            builder.RegisterWorkflow<CSharpSimpleTaskWorkflow>();
            builder.RegisterDynamicHandlers();
        }
    }
}
