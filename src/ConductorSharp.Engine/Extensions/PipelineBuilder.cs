using System;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Extensions;

internal class PipelineBuilder(IServiceCollection serviceCollection) : IPipelineBuilder
{
    public void AddRequestResponseLogging() =>
        serviceCollection.AddTransient(typeof(INgWorkerMiddleware<,>), typeof(RequestResponseLoggingMiddleware<,>));

    public void AddValidation() => serviceCollection.AddTransient(typeof(INgWorkerMiddleware<,>), typeof(ValidationWorkerMiddleware<,>));

    public void AddContextLogging() => serviceCollection.AddTransient(typeof(INgWorkerMiddleware<,>), typeof(ContextLoggingMiddleware<,>));

    public void AddExecutionTaskTracking() =>
        serviceCollection.AddTransient(typeof(INgWorkerMiddleware<,>), typeof(TaskExecutionTrackingMiddleware<,>));

    public void AddCustomMiddleware(Type middlewareType) => serviceCollection.AddTransient(typeof(INgWorkerMiddleware<,>), middlewareType);

    public void AddCustomMiddleware<TWorkerMiddleware, TRequest, TResponse>()
        where TWorkerMiddleware : class, INgWorkerMiddleware<TRequest, TResponse>
        where TRequest : class, ITaskInput<TResponse>, new() =>
        serviceCollection.AddTransient<INgWorkerMiddleware<TRequest, TResponse>, TWorkerMiddleware>();
}
