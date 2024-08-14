using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ConductorSharp.Engine.Util.Builders;

namespace ConductorSharp.Engine.Extensions
{
    public interface IConductorSharpBuilder
    {
        IExecutionManagerBuilder AddExecutionManager(
            int maxConcurrentWorkers,
            int sleepInterval,
            int longPollInterval,
            string domain = null,
            params Assembly[] handlerAssemblies
        );
        IConductorSharpBuilder SetBuildConfiguration(BuildConfiguration buildConfiguration);
        IConductorSharpBuilder AddAlternateClient(string baseUrl, string key, string apiPath = "api", bool ignoreInvalidCertificate = false);
    }
}
