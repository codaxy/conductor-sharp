using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Extensions
{
    public interface IExecutionManagerBuilder
    {
        IExecutionManagerBuilder AddPipelines(Action<IPipelineBuilder> pipelines);
    }
}
