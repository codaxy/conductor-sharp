using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Extensions
{
    public interface IConfigurableWorkflowEngineBuilder
    {
        IConfigurableWorkflowEngineExecutionManager AddExecutionManager(
            int maxConcurrentWorkers,
            int sleepInterval,
            int longPollInterval,
            string domain = null
        );
    }
}
