using System;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Extensions;

internal class PipelineBuilder(IServiceCollection serviceCollection) : IPipelineBuilder
{
    public void AddValidation() => serviceCollection.AddTransient(typeof(IWorkerMiddleware<,>), typeof(ValidationWorkerMiddleware<,>));

    public void AddExecutionTaskTracking() =>
        serviceCollection.AddTransient(typeof(IWorkerMiddleware<,>), typeof(TaskExecutionTrackingMiddleware<,>));

    public void AddCustomMiddleware(Type middlewareType) => serviceCollection.AddTransient(typeof(IWorkerMiddleware<,>), middlewareType);

    public void AddCustomMiddleware<TWorkerMiddleware, TRequest, TResponse>()
        where TWorkerMiddleware : class, IWorkerMiddleware<TRequest, TResponse>
        where TRequest : ITaskInput<TResponse>, new() => serviceCollection.AddTransient<IWorkerMiddleware<TRequest, TResponse>, TWorkerMiddleware>();
}
