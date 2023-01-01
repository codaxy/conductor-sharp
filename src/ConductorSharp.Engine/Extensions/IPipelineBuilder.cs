using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Extensions
{
    public interface IPipelineBuilder
    {
        void AddRequestResponseLogging();
        void AddValidation();
        void AddContextLogging();
        void AddExecutionTaskTracking();
    }
}
