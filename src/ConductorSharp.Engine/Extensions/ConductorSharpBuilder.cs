using Autofac;
using ConductorSharp.Engine.Behaviors;
using ConductorSharp.Engine.Handlers;
using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Polling;
using ConductorSharp.Engine.Service;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ConductorSharp.Engine.Extensions
{
    public class ConductorSharpBuilder : IConductorSharpBuilder, IExecutionManagerBuilder, IPipelineBuilder
    {
        public ContainerBuilder Builder { get; set; }

        public ConductorSharpBuilder(ContainerBuilder builder) => Builder = builder;

        public IExecutionManagerBuilder AddExecutionManager(int maxConcurrentWorkers, int sleepInterval, int longPollInterval, string domain = null)
        {
            var workerConfig = new WorkerSetConfig
            {
                MaxConcurrentWorkers = maxConcurrentWorkers,
                LongPollInterval = longPollInterval,
                Domain = domain,
                SleepInterval = sleepInterval
            };

            Builder.RegisterInstance(workerConfig).SingleInstance();

            Builder.RegisterType<WorkflowEngineBackgroundService>().As<IHostedService>();

            Builder.RegisterType<DeploymentService>().As<IDeploymentService>();

            Builder.RegisterType<ModuleDeployment>();

            Builder.RegisterType<ExecutionManager>().SingleInstance();

            Builder.RegisterType<ConductorSharpExecutionContext>().InstancePerLifetimeScope();

            Builder.RegisterType<InMemoryHealthService>().As<IConductorSharpHealthService>().SingleInstance();

            Builder.RegisterType<InverseExponentialBackoff>().As<IPollTimingStrategy>();

            Builder.RegisterType<RandomOrdering>().As<IPollOrderStrategy>();

            return this;
        }

        public IExecutionManagerBuilder AddPipelines(Action<IPipelineBuilder> behaviorBuilder)
        {
            behaviorBuilder(this);
            return this;
        }

        public void AddRequestResponseLogging() =>
            Builder.RegisterGeneric(typeof(RequestResponseLoggingBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        public void AddValidation() => Builder.RegisterGeneric(typeof(ValidationBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        public void AddContextLogging() => Builder.RegisterGeneric(typeof(ContextLoggingBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        public void AddExecutionTaskTracking() => Builder.RegisterGeneric(typeof(TaskExecutionTrackingBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        public IExecutionManagerBuilder SetHealthCheckService<T>() where T : IConductorSharpHealthService
        {
            Builder.RegisterType<T>().As<IConductorSharpHealthService>().SingleInstance();
            return this;
        }

        public IConductorSharpBuilder SetBuildConfiguration(BuildConfiguration buildConfiguration)
        {
            if (buildConfiguration is null)
            {
                throw new ArgumentNullException("Build configuration cannot be null");
            }

            Builder.RegisterInstance(buildConfiguration);
            return this;
        }

        public IExecutionManagerBuilder AddCSharpLambdaTasks(string csharpLambdaTaskNamePrefix = "")
        {
            Builder.RegisterWorkerTask<CSharpLambdaTaskHandler>();
            Builder.RegisterInstance(
                new ConfigurationProperty(CSharpLambdaTaskHandler.LambdaTaskNameConfigurationProperty, csharpLambdaTaskNamePrefix)
            );
            return this;
        }
    }
}
