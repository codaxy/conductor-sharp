using Autofac;
using ConductorSharp.Client;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Builders.Configurable;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConductorSharp.Engine.Extensions
{
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// This method is deprecated. Use AddConductorSharp method instead. WARNING: AddExecutionManager method will change when you move to AddConductorSharp,
        /// it will no longer register validation pipeline by default, and you will have to do it manually using the AddPipelines method. Check
        /// <see href="https://github.com/codaxy/conductor-sharp/blob/master/examples/ConductorSharp.ApiEnabled/Extensions/HostConfiguration.cs">example</see>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="baseUrl"></param>
        /// <param name="apiPath"></param>
        /// <param name="preventErrorOnBadRequest"></param>
        /// <param name="createClient"></param>
        /// <returns></returns>
        [Obsolete("Use AddConductorSharp method instead")]
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

        public static IConductorSharpBuilder AddConductorSharp(
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

            return new ConductorSharpBuilder(builder);
        }

        public static void RegisterWorkerTask<TWorkerTask>(this ContainerBuilder builder, Action<TaskDefinitionOptions> updateOptions = null)
            where TWorkerTask : IWorker
        {
            var taskDefinition = Builders.TaskDefinitionBuilder.Build<TWorkerTask>(updateOptions);

            builder.RegisterType<TWorkerTask>().Keyed<IWorker>(taskDefinition.Name);
            builder.RegisterInstance(taskDefinition);
            builder.RegisterInstance(new TaskToWorker { TaskName = taskDefinition.Name, TaskType = typeof(TWorkerTask), });
        }

        public static void RegisterWorkerTaskV2<TWorkerTask>(
            this ContainerBuilder builder,
            Action<TaskDefinitionOptions> updateOptions = null,
            BuildConfiguration buildConfiguration = null
        ) where TWorkerTask : IWorker
        {
            try
            {
                builder
                    .Register(ctx =>
                    {
                        return ctx.Resolve<Builders.Configurable.TaskDefinitionBuilder>().Build<TWorkerTask>();
                    })
                    .SingleInstance();

                //builder
                //    .Register(ctx =>
                //    {
                //        var definition = ctx.Resolve<Builders.Configurable.TaskDefinitionBuilder>().Build<TWorkerTask>();
                //        return definition.Name;
                //    })
                //    .As<TWorkerTask>();

                builder.Register(ctx =>
                {
                    var definition = ctx.Resolve<Builders.Configurable.TaskDefinitionBuilder>().Build<TWorkerTask>();
                    return new TaskToWorker { TaskName = definition.Name, TaskType = typeof(TWorkerTask) };
                });
            }
            catch (Exception exc) { }
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

        public static void RegisterWorkflow<TWorkflow>(this ContainerBuilder builder, BuildConfiguration buildConfiguration = null)
            where TWorkflow : IConfigurableWorkflow
        {
            builder.Register(ctx =>
            {
                var builder = ctx.Resolve(typeof(WorkflowDefinitionBuilder<,,>).MakeGenericType(typeof(TWorkflow).BaseType.GenericTypeArguments));

                if (buildConfiguration != null)
                {
                    builder.GetType().GetProperty("BuildConfiguration").SetValue(builder, buildConfiguration);
                }

                if (typeof(TWorkflow).GetMatchingConstructor(new Type[] { builder.GetType() }) == null)
                {
                    throw new ArgumentException($"Configurable workflow constructor must have a {builder.GetType().Name} parameter");
                }

                var workflow = Activator.CreateInstance(typeof(TWorkflow), builder) as ITypedWorkflow;

                var definition = workflow.GetDefinition();
                return definition;
            });
        }

        public static void RegisterTaskDefinition(this ContainerBuilder builder, TaskDefinition definition) => builder.RegisterInstance(definition);
        // TODO: add RegisterEventHandlerDefinition
    }
}
