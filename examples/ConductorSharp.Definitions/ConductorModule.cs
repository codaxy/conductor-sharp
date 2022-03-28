using Autofac;
using ConductorSharp.Definitions.Workflows;
using ConductorSharp.Engine.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Definitions
{
    public class ConductorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterWorkflow<SendCustomerNotification>();
        }
    }
}
