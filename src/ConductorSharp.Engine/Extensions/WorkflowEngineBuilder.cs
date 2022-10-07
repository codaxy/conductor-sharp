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

namespace ConductorSharp.Engine.Extensions
{
    public class WorkflowEngineBuilder : IWorkflowEngineBuilder, IWorkflowEngineExecutionManager
    {
        private readonly ContainerBuilder _builder;

        public WorkflowEngineBuilder(ContainerBuilder builder) => _builder = builder;

        public IWorkflowEngineExecutionManager AddExecutionManager(
            int maxConcurrentWorkers,
            int sleepInterval,
            int longPollInterval,
            string domain = null
        )
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

            _builder.RegisterGeneric(typeof(ValidationBehavior<,>)).As(typeof(IPipelineBehavior<,>));

            _builder.RegisterType<ConductorSharpHealthService>().AsImplementedInterfaces();
            _builder.RegisterType<InverseExponentialBackoff>().As<IPollTimingStrategy>();

            _builder.RegisterType<RandomOrdering>().As<IPollOrderStrategy>();

            return this;
        }
    }
}
