using Autofac;
using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Util.Builders;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ConductorSharp.Engine.Extensions
{
    public interface IExecutionManagerBuilder
    {
        ContainerBuilder Builder { get; set; }
        IExecutionManagerBuilder AddPipelines(Action<IPipelineBuilder> pipelines);
        IExecutionManagerBuilder SetHealthCheckService<T>() where T : IConductorSharpHealthService;
        IExecutionManagerBuilder AddCSharpLambdaTasks(string csharpLambdaTaskNamePrefix = "");
    }
}
