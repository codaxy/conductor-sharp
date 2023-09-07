using Autofac;
using ConductorSharp.Definitions.Workflows;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Util.Builders;

namespace ConductorSharp.Definitions
{
    public class ConductorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterWorkflow<SendCustomerNotification>();
            /*   builder.RegisterWorkflow<HandleNotificationFailure>();
               builder.RegisterWorkflow<CSharpLambdaWorkflow>();*/
        }
    }
}
