using Autofac;
using ConductorSharp.Client;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;

namespace ConductorSharp.Engine.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static IWorkflowEngineBuilder AddWorkflowEngine(
            this ContainerBuilder builder,
            string baseUrl,
            string apiPath,
            bool preventErrorOnBadRequest = false,
            Func<RestClient> createClient = null
        )
        {
            builder.RegisterInstance(
                new RestConfig
                {
                    ApiPath = apiPath,
                    BaseUrl = baseUrl,
                    CreateClient = createClient,
                    IgnoreValidationErrors = preventErrorOnBadRequest
                }
            );

            builder.RegisterType<ConductorClient>().As<IConductorClient>().SingleInstance();

            builder.RegisterType<TaskService>().As<ITaskService>();

            builder.RegisterType<HealthService>().As<IHealthService>();

            builder.RegisterType<MetadataService>().As<IMetadataService>();

            builder.RegisterType<WorkflowService>().As<IWorkflowService>();

            return new WorkflowEngineBuilder(builder);
        }

        public static void RegisterWorkerTask<TWorkerTask>(this ContainerBuilder builder, Action<TaskDefinitionOptions> updateOptions = null)
            where TWorkerTask : IWorker
        {
            var taskDefinition = TaskDefinitionBuilder.Build<TWorkerTask>(updateOptions);

            builder.RegisterType<TWorkerTask>().Keyed<IWorker>(taskDefinition.Name);
            builder.RegisterInstance(taskDefinition);
            builder.RegisterInstance(new TaskToWorker { TaskName = taskDefinition.Name, TaskType = typeof(TWorkerTask), });
        }

        public static void RegisterWorkflowDefinition(this ContainerBuilder builder, WorkflowDefinition definition) =>
            builder.RegisterInstance(definition);

        public static void RegisterWorkflowDefinition(this ContainerBuilder builder, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException($"'{nameof(filename)}' cannot be null or empty.", nameof(filename));

            var fileContents = File.ReadAllText(filename);
            var definition = JsonConvert.DeserializeObject<WorkflowDefinition>(fileContents);

            builder.RegisterInstance(definition);
        }

        public static void RegisterWorkflow<TWorkflow>(this ContainerBuilder builder) where TWorkflow : ITypedWorkflow, new() =>
            builder.RegisterInstance(new TWorkflow().GetDefinition());

        public static void RegisterTaskDefinition(this ContainerBuilder builder, TaskDefinition definition) => builder.RegisterInstance(definition);
        // TODO: add RegisterEventHandlerDefinition
    }
}
