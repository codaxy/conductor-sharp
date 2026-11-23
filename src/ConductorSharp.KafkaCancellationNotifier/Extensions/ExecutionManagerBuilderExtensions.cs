using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Interface;
using ConductorSharp.KafkaCancellationNotifier.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ConductorSharp.KafkaCancellationNotifier.Extensions
{
    public static class ExecutionManagerBuilderExtensions
    {
        public static IExecutionManagerBuilder AddKafkaCancellationNotifier(
            this IExecutionManagerBuilder builder,
            string kafkaBootstrapServers,
            string topicName,
            string groupId,
            bool createTopicOnStartup = false
        )
        {
            ArgumentNullException.ThrowIfNull(kafkaBootstrapServers);
            ArgumentNullException.ThrowIfNull(topicName);
            ArgumentNullException.ThrowIfNull(groupId);

            builder
                .Builder.AddOptions<KafkaOptions>()
                .Configure(opts =>
                {
                    opts.BootstrapServers = kafkaBootstrapServers;
                    opts.GroupId = groupId;
                    opts.TopicName = topicName;
                    opts.CreateTopicOnStartup = createTopicOnStartup;
                });

            return AddNotifierServices(builder);
        }

        /// <summary>
        /// Registers the Kafka cancellation notifier with settings bound from the given configuration section
        /// (e.g. <c>Conductor:KafkaCancellationNotifier</c>), including SSL, mTLS and SASL settings.
        /// Settings are validated on startup.
        /// </summary>
        public static IExecutionManagerBuilder AddKafkaCancellationNotifier(this IExecutionManagerBuilder builder, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            builder.Builder.AddOptions<KafkaOptions>().Bind(configuration).ValidateOnStart();
            builder.Builder.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<KafkaOptions>, KafkaOptionsValidator>());

            return AddNotifierServices(builder);
        }

        private static IExecutionManagerBuilder AddNotifierServices(IExecutionManagerBuilder builder)
        {
            builder.Builder.AddSingleton<ICancellationNotifier, Service.KafkaCancellationNotifier>();
            builder.Builder.AddHostedService<KafkaConsumerBackgroundService>();

            return builder;
        }
    }
}
