using System;
using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Extensions
{
    public interface IPipelineBuilder
    {
        void AddValidation();
        void AddExecutionTaskTracking();
        void AddCustomMiddleware(Type middlewareType);

        void AddCustomMiddleware<TWorkerMiddleware, TRequest, TResponse>()
            where TWorkerMiddleware : class, INgWorkerMiddleware<TRequest, TResponse>
            where TRequest : class, ITaskInput<TResponse>, new();
    }
}
