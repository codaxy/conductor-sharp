using System;
using ConductorSharp.Engine.Behaviors;
using ConductorSharp.Engine.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Extensions;

internal class PipelineBuilder(IServiceCollection serviceCollection) : IPipelineBuilder
{
    public void AddRequestResponseLogging() =>
        serviceCollection.AddTransient(typeof(INgWorkerMiddleware<,>), typeof(RequestResponseLoggingBehavior<,>));

    public void AddValidation() => serviceCollection.AddTransient(typeof(INgWorkerMiddleware<,>), typeof(ValidationWorkerMiddleware<,>));

    public void AddContextLogging() => serviceCollection.AddTransient(typeof(INgWorkerMiddleware<,>), typeof(ContextLoggingBehavior<,>));

    public void AddExecutionTaskTracking() =>
        serviceCollection.AddTransient(typeof(INgWorkerMiddleware<,>), typeof(TaskExecutionTrackingBehavior<,>));

    public void AddCustomBehavior(Type behaviorType) => serviceCollection.AddTransient(typeof(INgWorkerMiddleware<,>), behaviorType);

    public void AddCustomBehavior<TWorkerMiddleware, TRequest, TResponse>()
        where TWorkerMiddleware : class, INgWorkerMiddleware<TRequest, TResponse>
        where TRequest : class, ITaskInput<TResponse>, new()
    {
        serviceCollection.AddTransient<INgWorkerMiddleware<TRequest, TResponse>, TWorkerMiddleware>();
    }
}
