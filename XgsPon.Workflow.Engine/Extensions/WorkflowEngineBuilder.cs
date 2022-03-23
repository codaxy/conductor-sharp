using Autofac;
using MediatR;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using XgsPon.Workflows.Engine.Behaviors;
using XgsPon.Workflows.Engine.Interface;
using XgsPon.Workflows.Engine.Service;
using XgsPon.Workflows.Engine;

namespace XgsPon.Workflows.Engine.Extensions
{
    public interface IWorkflowEngineBuilder
    {
        IWorkflowEngineExecutionManager AddExecutionManager(
            int maxConcurrentWorkers,
            int sleepInterval,
            int longPollInterval,
            string domain = null
        );
    }

    public interface IWorkflowEngineExecutionManager
    {
    }

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

            _builder
                .RegisterGeneric(typeof(ValidationBehavior<,>))
                .As(typeof(IPipelineBehavior<,>));

            //_builder.RegisterGeneric(typeof(ErrorHandlingBehavior<, >)).As(typeof(IPipelineBehavior<, >));

            return this;
        }
    }
}
