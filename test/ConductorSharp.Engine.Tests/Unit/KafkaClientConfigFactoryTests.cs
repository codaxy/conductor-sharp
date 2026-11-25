using ConductorSharp.KafkaCancellationNotifier;
using ConductorSharp.KafkaCancellationNotifier.Service;
using Confluent.Kafka;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class KafkaClientConfigFactoryTests
    {
        private static KafkaOptions CreateOptions() =>
            new()
            {
                BootstrapServers = "broker:9093",
                TopicName = "conductor.status.task",
                GroupId = "group"
            };

        [Fact]
        public void PlaintextConsumerConfigContainsOnlyPreviouslySetProperties()
        {
            var config = KafkaClientConfigFactory.CreateConsumerConfig(CreateOptions());

            Assert.Equal(
                new Dictionary<string, string>
                {
                    ["bootstrap.servers"] = "broker:9093",
                    ["group.id"] = "group",
                    ["auto.offset.reset"] = "latest"
                },
                config.ToDictionary(kv => kv.Key, kv => kv.Value)
            );
        }

        [Fact]
        public void PlaintextAdminClientConfigContainsOnlyBootstrapServers()
        {
            var config = KafkaClientConfigFactory.CreateAdminClientConfig(CreateOptions());

            Assert.Equal(new Dictionary<string, string> { ["bootstrap.servers"] = "broker:9093" }, config.ToDictionary(kv => kv.Key, kv => kv.Value));
        }

        [Fact]
        public void SslSettingsAreMapped()
        {
            var options = CreateOptions();
            options.SecurityProtocol = SecurityProtocol.Ssl;
            options.SslCaLocation = "/certs/ca.pem";
            options.EnableSslCertificateVerification = false;
            options.SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None;

            var config = KafkaClientConfigFactory.CreateConsumerConfig(options);

            Assert.Equal(SecurityProtocol.Ssl, config.SecurityProtocol);
            Assert.Equal("/certs/ca.pem", config.SslCaLocation);
            Assert.False(config.EnableSslCertificateVerification);
            Assert.Equal(SslEndpointIdentificationAlgorithm.None, config.SslEndpointIdentificationAlgorithm);
            Assert.Null(config.SaslMechanism);
            Assert.Null(config.SslCertificateLocation);
        }

        [Fact]
        public void MutualTlsPemSettingsAreMapped()
        {
            var options = CreateOptions();
            options.SecurityProtocol = SecurityProtocol.Ssl;
            options.SslCaLocation = "/certs/ca.pem";
            options.SslCertificateLocation = "/certs/client.pem";
            options.SslKeyLocation = "/certs/client.key";
            options.SslKeyPassword = "key-password";

            var config = KafkaClientConfigFactory.CreateConsumerConfig(options);

            Assert.Equal("/certs/client.pem", config.SslCertificateLocation);
            Assert.Equal("/certs/client.key", config.SslKeyLocation);
            Assert.Equal("key-password", config.SslKeyPassword);
            Assert.Null(config.SslKeystoreLocation);
        }

        [Fact]
        public void MutualTlsKeystoreSettingsAreMapped()
        {
            var options = CreateOptions();
            options.SecurityProtocol = SecurityProtocol.Ssl;
            options.SslKeystoreLocation = "/certs/client.p12";
            options.SslKeystorePassword = "keystore-password";

            var config = KafkaClientConfigFactory.CreateConsumerConfig(options);

            Assert.Equal("/certs/client.p12", config.SslKeystoreLocation);
            Assert.Equal("keystore-password", config.SslKeystorePassword);
            Assert.Null(config.SslCertificateLocation);
        }

        [Fact]
        public void SaslPlaintextSettingsAreMapped()
        {
            var options = CreateOptions();
            options.SecurityProtocol = SecurityProtocol.SaslPlaintext;
            options.SaslMechanism = SaslMechanism.Plain;
            options.SaslUsername = "user";
            options.SaslPassword = "password";

            var config = KafkaClientConfigFactory.CreateConsumerConfig(options);

            Assert.Equal(SecurityProtocol.SaslPlaintext, config.SecurityProtocol);
            Assert.Equal(SaslMechanism.Plain, config.SaslMechanism);
            Assert.Equal("user", config.SaslUsername);
            Assert.Equal("password", config.SaslPassword);
            Assert.Null(config.SslCaLocation);
        }

        [Fact]
        public void SaslSslSettingsAreMappedToConsumerAndAdminClient()
        {
            var options = CreateOptions();
            options.SecurityProtocol = SecurityProtocol.SaslSsl;
            options.SaslMechanism = SaslMechanism.ScramSha512;
            options.SaslUsername = "user";
            options.SaslPassword = "password";
            options.SslCaLocation = "/certs/ca.pem";

            var configs = new ClientConfig[]
            {
                KafkaClientConfigFactory.CreateConsumerConfig(options),
                KafkaClientConfigFactory.CreateAdminClientConfig(options)
            };

            Assert.All(
                configs,
                config =>
                {
                    Assert.Equal("broker:9093", config.BootstrapServers);
                    Assert.Equal(SecurityProtocol.SaslSsl, config.SecurityProtocol);
                    Assert.Equal(SaslMechanism.ScramSha512, config.SaslMechanism);
                    Assert.Equal("user", config.SaslUsername);
                    Assert.Equal("password", config.SaslPassword);
                    Assert.Equal("/certs/ca.pem", config.SslCaLocation);
                }
            );
        }

        [Theory]
        [InlineData(SecurityProtocol.Plaintext)]
        [InlineData(SecurityProtocol.Ssl)]
        [InlineData(SecurityProtocol.SaslPlaintext)]
        [InlineData(SecurityProtocol.SaslSsl)]
        public void LibrdkafkaAcceptsGeneratedConfig(SecurityProtocol protocol)
        {
            var options = CreateOptions();
            options.SecurityProtocol = protocol;
            if (protocol is SecurityProtocol.SaslPlaintext or SecurityProtocol.SaslSsl)
            {
                options.SaslMechanism = SaslMechanism.ScramSha256;
                options.SaslUsername = "user";
                options.SaslPassword = "password";
            }
            if (protocol is SecurityProtocol.Ssl or SecurityProtocol.SaslSsl)
            {
                options.EnableSslCertificateVerification = false;
                options.SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None;
            }

            // Building clients makes librdkafka validate the configuration without connecting to a broker
            using var consumer = new ConsumerBuilder<string, string>(KafkaClientConfigFactory.CreateConsumerConfig(options)).Build();
            using var admin = new AdminClientBuilder(KafkaClientConfigFactory.CreateAdminClientConfig(options)).Build();
        }
    }
}
