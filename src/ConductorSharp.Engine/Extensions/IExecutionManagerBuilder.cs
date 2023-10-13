using ConductorSharp.Engine.Health;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ConductorSharp.Engine.Extensions
{
    public interface IExecutionManagerBuilder
    {
        IServiceCollection Builder { get; set; }
        IExecutionManagerBuilder AddPipelines(Action<IPipelineBuilder> pipelines);
        IExecutionManagerBuilder SetHealthCheckService<T>() where T : IConductorSharpHealthService;
    }
}
