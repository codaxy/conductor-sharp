using Autofac;
using ConductorSharp.Engine.Behaviors;
using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Polling;
using ConductorSharp.Engine.Service;
using ConductorSharp.Engine.Util;
using MediatR;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Extensions
{
    public class ConductorSharpBuilder : IConductorSharpBuilder, IExecutionManagerBuilder, IPipelineBuilder
    {
        private readonly ContainerBuilder _builder;

        public ConductorSharpBuilder(ContainerBuilder builder) => _builder = builder;

        public IExecutionManagerBuilder AddExecutionManager(int maxConcurrentWorkers, int sleepInterval, int longPollInterval, string domain = null)
        {
            var workerConfig = new WorkerSetConfig
            {
                MaxConcurrentWorkers = maxConcurrentWorkers,
                LongPollInterval = longPollInterval,
                Domain = domain,
                SleepInterval = sleepInterval
            };

            _builder.RegisterInstance(workerConfig).SingleInstance();

            _builder.RegisterType<WorkflowEngineBackgroundService>().As<IHostedService>();

            _builder.RegisterType<DeploymentService>().As<IDeploymentService>();

            _builder.RegisterType<ModuleDeployment>();

            _builder.RegisterType<ExecutionManager>().SingleInstance();

            _builder.RegisterType<ConductorSharpExecutionContext>().InstancePerLifetimeScope();

            _builder.RegisterType<InMemoryHealthService>().As<IConductorSharpHealthService>();

            _builder.RegisterType<InverseExponentialBackoff>().As<IPollTimingStrategy>();

            _builder.RegisterType<RandomOrdering>().As<IPollOrderStrategy>();

            return this;
        }

        public IExecutionManagerBuilder AddPipelines(Action<IPipelineBuilder> behaviorBuilder)
        {
            behaviorBuilder(this);
            return this;
        }

        public void AddRequestResponseLogging() =>
            _builder.RegisterGeneric(typeof(RequestResponseLoggingBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        public void AddValidation() => _builder.RegisterGeneric(typeof(ValidationBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        public void AddContextLogging() => _builder.RegisterGeneric(typeof(ContextLoggingBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        public IExecutionManagerBuilder SetHealthCheckService<T>() where T : IConductorSharpHealthService
        {
            _builder.RegisterType<T>().As<IConductorSharpHealthService>();
            return this;
        }
    }
}
