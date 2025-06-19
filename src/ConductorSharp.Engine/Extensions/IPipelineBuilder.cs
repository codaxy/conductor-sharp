using System;
using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Extensions
{
    public interface IPipelineBuilder
    {
        void AddRequestResponseLogging();
        void AddValidation();
        void AddContextLogging();
        void AddExecutionTaskTracking();
        void AddCustomBehavior(Type behaviorType);
        void AddCustomBehavior<TWorkerMiddleware, TRequest, TResponse>()
            where TWorkerMiddleware : class, INgWorkerMiddleware<TRequest, TResponse>
            where TRequest : class, new()
            where TResponse : class, new();
    }
}
