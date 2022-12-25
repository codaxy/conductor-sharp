using Autofac;
using ConductorSharp.Client;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Builders.Configurable;
using ConductorSharp.Engine.Util.Builders;
using RestSharp;
using System;

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

            builder.RegisterInstance(new BuildConfiguration());

            builder.RegisterType<WorkflowBuildItemRegistry>().SingleInstance();

            builder.RegisterType<TaskDefinitionBuilder>();

            builder.RegisterGeneric(typeof(WorkflowDefinitionBuilder<,,>));

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

            builder.RegisterInstance(new BuildConfiguration());

            builder.RegisterType<WorkflowBuildItemRegistry>().SingleInstance();

            builder.RegisterType<TaskDefinitionBuilder>();

            builder.RegisterGeneric(typeof(WorkflowDefinitionBuilder<,,>));

            return new ConductorSharpBuilder(builder);
        }
    }
}
