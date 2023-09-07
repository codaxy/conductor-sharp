using Autofac;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.NoApi.Handlers;

namespace ConductorSharp.NoApi
{
    public class ConductorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            RegisterTasks(builder);
            RegisterWorkflows(builder);
        }

        private static void RegisterTasks(ContainerBuilder builder)
        {
            builder.RegisterWorkerTask<GetCustomerHandler>(options =>
            {
                options.OwnerEmail = "owneremail@gmail.com";
            });
        }

        private static void RegisterWorkflows(ContainerBuilder builder) { }
    }
}
