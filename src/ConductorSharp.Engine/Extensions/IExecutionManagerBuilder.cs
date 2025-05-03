using System;
using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Polling;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Extensions
{
    public interface IExecutionManagerBuilder
    {
        IServiceCollection Builder { get; set; }
        IExecutionManagerBuilder AddPipelines(Action<IPipelineBuilder> pipelines);
        IExecutionManagerBuilder SetHealthCheckService<T>()
            where T : IConductorSharpHealthService;
        IExecutionManagerBuilder UseConstantPollTimingStrategy();
        IExecutionManagerBuilder UseBetaExecutionManager();
    }
}
