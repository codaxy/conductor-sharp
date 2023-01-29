using Autofac;
using ConductorSharp.Engine.Health;
using System;

namespace ConductorSharp.Engine.Extensions
{
    public interface IExecutionManagerBuilder
    {
        ContainerBuilder Builder { get; set; }
        IExecutionManagerBuilder AddPipelines(Action<IPipelineBuilder> pipelines);
        IExecutionManagerBuilder SetHealthCheckService<T>() where T : IConductorSharpHealthService;
    }
}
