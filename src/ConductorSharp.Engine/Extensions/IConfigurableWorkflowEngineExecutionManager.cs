using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Extensions
{
    public interface IConfigurableWorkflowEngineExecutionManager
    {
        IConfigurableWorkflowEngineExecutionManager AddPipelines(Action<IPipelineBuilder> pipelines);
    }
}
