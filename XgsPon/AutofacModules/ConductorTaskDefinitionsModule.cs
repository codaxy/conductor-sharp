using Autofac;
using XgsPn.Handlers;
using XgsPon.Workflows.Engine.Extensions;

namespace XgsPon.AutofacModules
{
    public class ConductorTaskDefinitionsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterWorkerTask<TestHandler>();
        }
    }
}
