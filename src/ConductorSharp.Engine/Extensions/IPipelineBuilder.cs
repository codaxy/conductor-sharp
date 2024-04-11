using System;
using MediatR;

namespace ConductorSharp.Engine.Extensions
{
    public interface IPipelineBuilder
    {
        void AddRequestResponseLogging();
        void AddValidation();
        void AddContextLogging();
        void AddExecutionTaskTracking();
        void AddCustomBehavior(Type behaviorType);
        void AddCustomBehavior<TBehavior, TRequest, TResponse>()
            where TBehavior : class, IPipelineBehavior<TRequest, TResponse>;
    }
}
