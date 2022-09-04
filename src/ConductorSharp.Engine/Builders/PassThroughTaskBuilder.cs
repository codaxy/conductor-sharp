using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Builders
{
    public class PassThroughTaskBuilder : ITaskBuilder
    {
        private readonly WorkflowDefinition.Task[] _tasks;

        public PassThroughTaskBuilder(WorkflowDefinition.Task[] tasks)
        {
            _tasks = tasks;
        }

        public WorkflowDefinition.Task[] Build() => _tasks;
    }
}
