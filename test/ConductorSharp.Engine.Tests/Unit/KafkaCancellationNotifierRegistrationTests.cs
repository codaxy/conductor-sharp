using ConductorSharp.Engine.Extensions;
using ConductorSharp.KafkaCancellationNotifier;
using ConductorSharp.KafkaCancellationNotifier.Extensions;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class KafkaCancellationNotifierRegistrationTests
    {
        private const string SectionName = "Conductor:KafkaCancellationNotifier";

        private static (IServiceCollection Services, IExecutionManagerBuilder Builder) CreateBuilder()
        {
            var services = new ServiceCollection();
            var builder = services
                .AddConductorSharp(baseUrl: "http://empty/empty")
                .AddExecutionManager(
                    maxConcurrentWorkers: 1,
                    sleepInterval: 1,
                    longPollInterval: 1,
                    domain: null,
                    handlerAssemblies: typeof(KafkaCancellationNotifierRegistrationTests).Assembly
                );

            return (services, builder);
        }

        private static IConfiguration CreateSection(Dictionary<string, string> settings) =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(settings.ToDictionary(kv => $"{SectionName}:{kv.Key}", kv => kv.Value))
                .Build()
                .GetSection(SectionName);

        private static Dictionary<string, string> RequiredSettings() =>
            new()
            {
                ["BootstrapServers"] = "broker:9093",
                ["TopicName"] = "conductor.status.task",
                ["GroupId"] = "group"
            };

        [Fact]
        public void ExistingMethodRegistersOptionsWithoutValidation()
        {
            var (services, builder) = CreateBuilder();
            builder.AddKafkaCancellationNotifier(kafkaBootstrapServers: "broker:9092", topicName: "topic", groupId: "", createTopicOnStartup: true);

            using var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;

            Assert.Equal("broker:9092", options.BootstrapServers);
            Assert.Equal("topic", options.TopicName);
            Assert.Equal("", options.GroupId);
            Assert.True(options.CreateTopicOnStartup);
            Assert.Null(options.SecurityProtocol);
            Assert.False(options.HasSslSettings);
            Assert.Empty(provider.GetServices<IValidateOptions<KafkaOptions>>());
            Assert.Contains(services, d => d.ServiceType == typeof(ICancellationNotifier));
            Assert.Contains(services, d => d.ServiceType == typeof(IHostedService));
        }

        [Fact]
        public void ConfigurationOverloadBindsAllSettings()
        {
            var settings = RequiredSettings();
            settings["CreateTopicOnStartup"] = "true";
            settings["SecurityProtocol"] = "SaslSsl";
            settings["SaslMechanism"] = "ScramSha512";
            settings["SaslUsername"] = "user";
            settings["SaslPassword"] = "password";
            settings["SslKeyPassword"] = "key-password";
            settings["SslKeystorePassword"] = "keystore-password";
            settings["EnableSslCertificateVerification"] = "false";
            settings["SslEndpointIdentificationAlgorithm"] = "None";

            var (services, builder) = CreateBuilder();
            builder.AddKafkaCancellationNotifier(CreateSection(settings));

            using var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;

            Assert.Equal("broker:9093", options.BootstrapServers);
            Assert.Equal("conductor.status.task", options.TopicName);
            Assert.Equal("group", options.GroupId);
            Assert.True(options.CreateTopicOnStartup);
            Assert.Equal(SecurityProtocol.SaslSsl, options.SecurityProtocol);
            Assert.Equal(SaslMechanism.ScramSha512, options.SaslMechanism);
            Assert.Equal("user", options.SaslUsername);
            Assert.Equal("password", options.SaslPassword);
            Assert.Equal("key-password", options.SslKeyPassword);
            Assert.Equal("keystore-password", options.SslKeystorePassword);
            Assert.False(options.EnableSslCertificateVerification);
            Assert.Equal(SslEndpointIdentificationAlgorithm.None, options.SslEndpointIdentificationAlgorithm);
            Assert.Contains(services, d => d.ServiceType == typeof(ICancellationNotifier));
            Assert.Contains(services, d => d.ServiceType == typeof(IHostedService));
        }

        [Fact]
        public void ConfigurationOverloadLeavesOptionalSettingsUnset()
        {
            var (services, builder) = CreateBuilder();
            builder.AddKafkaCancellationNotifier(CreateSection(RequiredSettings()));

            using var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;

            Assert.False(options.CreateTopicOnStartup);
            Assert.Null(options.SecurityProtocol);
            Assert.Null(options.SaslMechanism);
            Assert.Null(options.SaslUsername);
            Assert.Null(options.SaslPassword);
            Assert.False(options.HasSslSettings);
        }

        [Fact]
        public void ConfigurationOverloadValidatesOnStartup()
        {
            var settings = RequiredSettings();
            settings["SecurityProtocol"] = "SaslPlaintext";

            var (services, builder) = CreateBuilder();
            builder.AddKafkaCancellationNotifier(CreateSection(settings));

            using var provider = services.BuildServiceProvider();
            var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());

            Assert.Contains($"{nameof(KafkaOptions.SaslUsername)} is required", exception.Failures);
        }

        [Fact]
        public void ConfigurationOverloadPassesStartupValidationWhenValid()
        {
            var (services, builder) = CreateBuilder();
            builder.AddKafkaCancellationNotifier(CreateSection(RequiredSettings()));

            using var provider = services.BuildServiceProvider();

            provider.GetRequiredService<IStartupValidator>().Validate();
        }
    }
}
