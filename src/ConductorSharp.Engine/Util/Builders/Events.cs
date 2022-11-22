using Autofac;
using ConductorSharp.Client.Model.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Util.Builders
{
    public class Events
    {
        public delegate void HandleRegistration(ContainerBuilder containerBuilder);
        public delegate void HandleResolve(IComponentContext componntContext);
        public delegate void HandleBuild(WorkflowDefinition workflowDefinition);
    }
}
