using Autofac;
using ConductorSharp.Client;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util.Builders;
using RestSharp;
using System;
using System.Net.Http;

namespace ConductorSharp.Engine.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static IConductorSharpBuilder AddConductorSharp(
            this ContainerBuilder builder,
            string baseUrl,
            string apiPath,
            bool preventErrorOnBadRequest = false,
            Func<RestClient> createClient = null
        )
        {
            /*          builder.RegisterInstance(
                          new RestConfig
                          {
                              ApiPath = apiPath,
                              BaseUrl = baseUrl,
                              CreateClient = createClient,
                              IgnoreValidationErrors = preventErrorOnBadRequest
                          }
                      );*/

            builder.RegisterType<HttpClient>().AsSelf();

            builder.RegisterType<ConductorHttpClient>().As<IConductorClient>().SingleInstance();

            builder.RegisterType<TaskService>().As<ITaskService>();

            builder.RegisterType<HealthService>().As<IHealthService>();

            builder.RegisterType<MetadataService>().As<IMetadataService>();

            builder.RegisterType<WorkflowService>().As<IWorkflowService>();

            builder.RegisterInstance(new BuildConfiguration());

            builder.RegisterType<WorkflowBuildItemRegistry>().SingleInstance();

            builder.RegisterType<DefaultTaskNameBuilder>().As<ITaskNameBuilder>();

            builder.RegisterType<TaskDefinitionBuilder>();

            builder.RegisterGeneric(typeof(WorkflowDefinitionBuilder<,,>));

            return new ConductorSharpBuilder(builder);
        }
    }
}
