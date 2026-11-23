using Confluent.Kafka;

namespace ConductorSharp.KafkaCancellationNotifier.Service
{
    internal static class KafkaClientConfigFactory
    {
        public static ConsumerConfig CreateConsumerConfig(KafkaOptions options)
        {
            var config = new ConsumerConfig { GroupId = options.GroupId, AutoOffsetReset = AutoOffsetReset.Latest };
            ApplyConnectionSettings(config, options);
            return config;
        }

        public static AdminClientConfig CreateAdminClientConfig(KafkaOptions options)
        {
            var config = new AdminClientConfig();
            ApplyConnectionSettings(config, options);
            return config;
        }

        private static void ApplyConnectionSettings(ClientConfig config, KafkaOptions options)
        {
            config.BootstrapServers = options.BootstrapServers;

            // Only configured values are set, so an unsecured setup produces exactly the same config as before
            if (options.SecurityProtocol is not null)
                config.SecurityProtocol = options.SecurityProtocol;

            if (options.SaslMechanism is not null)
                config.SaslMechanism = options.SaslMechanism;
            if (options.SaslUsername is not null)
                config.SaslUsername = options.SaslUsername;
            if (options.SaslPassword is not null)
                config.SaslPassword = options.SaslPassword;

            if (options.SslCaLocation is not null)
                config.SslCaLocation = options.SslCaLocation;
            if (options.SslCertificateLocation is not null)
                config.SslCertificateLocation = options.SslCertificateLocation;
            if (options.SslKeyLocation is not null)
                config.SslKeyLocation = options.SslKeyLocation;
            if (options.SslKeyPassword is not null)
                config.SslKeyPassword = options.SslKeyPassword;
            if (options.SslKeystoreLocation is not null)
                config.SslKeystoreLocation = options.SslKeystoreLocation;
            if (options.SslKeystorePassword is not null)
                config.SslKeystorePassword = options.SslKeystorePassword;
            if (options.EnableSslCertificateVerification is not null)
                config.EnableSslCertificateVerification = options.EnableSslCertificateVerification;
            if (options.SslEndpointIdentificationAlgorithm is not null)
                config.SslEndpointIdentificationAlgorithm = options.SslEndpointIdentificationAlgorithm;
        }
    }
}
