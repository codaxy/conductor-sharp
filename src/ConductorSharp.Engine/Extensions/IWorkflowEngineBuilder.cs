using ConductorSharp.Engine.Util.Builders;

namespace ConductorSharp.Engine.Extensions
{
    public interface IWorkflowEngineBuilder
    {
        IWorkflowEngineExecutionManager AddExecutionManager(int maxConcurrentWorkers, int sleepInterval, int longPollInterval, string domain = null);
        IWorkflowEngineBuilder SetBuildConfiguration(BuildConfiguration buildConfiguration);
    }
}
