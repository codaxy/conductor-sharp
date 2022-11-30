using Autofac;
using ConductorSharp.Client.Model.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Util.Builders
{
    public class Events
    {
        public delegate void LoadWorflow(ContainerBuilder containerBuilder);
        public delegate void ResolveWorkflow(IComponentContext componntContext);
        public delegate void GetWorkflowDefinition(WorkflowDefinition workflowDefinition);
    }
}
