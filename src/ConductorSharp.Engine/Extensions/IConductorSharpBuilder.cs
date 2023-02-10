using ConductorSharp.Engine.Util.Builders;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ConductorSharp.Engine.Extensions
{
    public interface IConductorSharpBuilder
    {
        IExecutionManagerBuilder AddExecutionManager(
            int maxConcurrentWorkers,
            int sleepInterval,
            int longPollInterval,
            Assembly handlersAssembly,
            string domain = null
        );
        IConductorSharpBuilder SetBuildConfiguration(BuildConfiguration buildConfiguration);
    }
}
