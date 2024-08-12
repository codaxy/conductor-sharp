using System;
using System.Reflection;
using ConductorSharp.Engine.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Extensions;

internal class PipelineBuilder(IServiceCollection serviceCollection) : IPipelineBuilder
{
    public void AddRequestResponseLogging() =>
        serviceCollection.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestResponseLoggingBehavior<,>));

    public void AddValidation() => serviceCollection.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    public void AddContextLogging() => serviceCollection.AddTransient(typeof(IPipelineBehavior<,>), typeof(ContextLoggingBehavior<,>));

    public void AddExecutionTaskTracking() => serviceCollection.AddTransient(typeof(IPipelineBehavior<,>), typeof(TaskExecutionTrackingBehavior<,>));

    public void AddCustomBehavior(Type behaviorType) => serviceCollection.AddTransient(typeof(IPipelineBehavior<,>), behaviorType);

    public void AddCustomBehavior<TBehavior, TRequest, TResponse>()
        where TBehavior : class, IPipelineBehavior<TRequest, TResponse>
    {
        serviceCollection.AddTransient<IPipelineBehavior<TRequest, TResponse>, TBehavior>();
    }
}
