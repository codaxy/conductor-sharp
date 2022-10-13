using ConductorSharp.Engine.Health;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Extensions
{
    public interface IExecutionManagerBuilder
    {
        IExecutionManagerBuilder AddPipelines(Action<IPipelineBuilder> pipelines);
        IExecutionManagerBuilder SetHealthCheckService<T>() where T : IConductorSharpHealthService;
        IExecutionManagerBuilder AddCSharpLambdaTasks();
    }
}
