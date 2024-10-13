using System;
using System.Net.Http;
using System.Reflection;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace ConductorSharp.Engine.Extensions
{
    public static class ContainerBuilderExtensions
    {
        private const string DefaultClientName = "ConductorSharp.Client.DefaultHttpClient";

        public static IConductorSharpBuilder AddConductorSharp(
            this IServiceCollection builder,
            string baseUrl,
            string apiPath = "api",
            bool ignoreInvalidCertificate = false
        )
        {
            var clientBuilder = builder
                .AddHttpClient(DefaultClientName, client => client.BaseAddress = new(baseUrl))
                .AddHttpMessageHandler(() => new ApiPathOverrideHttpHandler(apiPath));

            if (ignoreInvalidCertificate)
            {
                clientBuilder.ConfigurePrimaryHttpMessageHandler(
                    () => new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true }
                );
            }

            builder.AddTransient<IAdminService, AdminService>(sp => new(sp.GetService<IHttpClientFactory>().CreateClient(DefaultClientName)));
            builder.AddTransient<IEventService, EventService>(sp => new(sp.GetService<IHttpClientFactory>().CreateClient(DefaultClientName)));
            builder.AddTransient<IExternalPayloadService, ExternalPayloadService>(
                sp => new(sp.GetService<IHttpClientFactory>().CreateClient(DefaultClientName))
            );
            builder.AddTransient<IQueueAdminService, QueueAdminService>(
                sp => new(sp.GetService<IHttpClientFactory>().CreateClient(DefaultClientName))
            );
            builder.AddTransient<IWorkflowBulkService, WorkflowBulkService>(
                sp => new(sp.GetService<IHttpClientFactory>().CreateClient(DefaultClientName))
            );
            builder.AddTransient<ITaskService, TaskService>(sp => new(sp.GetService<IHttpClientFactory>().CreateClient(DefaultClientName)));
            builder.AddTransient<IHealthService, HealthService>(sp => new(sp.GetService<IHttpClientFactory>().CreateClient(DefaultClientName)));
            builder.AddTransient<IMetadataService, MetadataService>(sp => new(sp.GetService<IHttpClientFactory>().CreateClient(DefaultClientName)));
            builder.AddTransient<IWorkflowService, WorkflowService>(sp => new(sp.GetService<IHttpClientFactory>().CreateClient(DefaultClientName)));

            builder.AddSingleton(new BuildConfiguration());
            builder.AddSingleton<WorkflowBuildItemRegistry>();
            builder.AddTransient<ITaskNameBuilder, DefaultTaskNameBuilder>();
            builder.AddTransient<TaskDefinitionBuilder>();
            builder.AddTransient(typeof(WorkflowDefinitionBuilder<,,>));
            return new ConductorSharpBuilder(builder);
        }
    }
}
