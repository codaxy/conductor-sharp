using Autofac;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.NoApi.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            builder.RegisterWorkerTask<GetCustomerHandler>();
        }

        private static void RegisterWorkflows(ContainerBuilder builder) { }
    }
}
