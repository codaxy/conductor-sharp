using Confluent.Kafka;

namespace ConductorSharp.KafkaCancellationNotifier;

internal class KafkaOptions
{
    public string BootstrapServers { get; set; } = null!;
    public string TopicName { get; set; } = null!;
    public string GroupId { get; set; } = null!;
    public bool CreateTopicOnStartup { get; set; }

    // Security settings are optional and passed to librdkafka only when configured, so librdkafka defaults apply otherwise
    public SecurityProtocol? SecurityProtocol { get; set; }

    public SaslMechanism? SaslMechanism { get; set; }
    public string? SaslUsername { get; set; }
    public string? SaslPassword { get; set; }

    public string? SslCaLocation { get; set; }
    public string? SslCertificateLocation { get; set; }
    public string? SslKeyLocation { get; set; }
    public string? SslKeyPassword { get; set; }
    public string? SslKeystoreLocation { get; set; }
    public string? SslKeystorePassword { get; set; }
    public bool? EnableSslCertificateVerification { get; set; }
    public SslEndpointIdentificationAlgorithm? SslEndpointIdentificationAlgorithm { get; set; }

    internal bool IsSaslProtocol => SecurityProtocol is Confluent.Kafka.SecurityProtocol.SaslPlaintext or Confluent.Kafka.SecurityProtocol.SaslSsl;

    internal bool IsSslProtocol => SecurityProtocol is Confluent.Kafka.SecurityProtocol.Ssl or Confluent.Kafka.SecurityProtocol.SaslSsl;

    internal bool HasSslSettings =>
        SslCaLocation is not null
        || SslCertificateLocation is not null
        || SslKeyLocation is not null
        || SslKeyPassword is not null
        || SslKeystoreLocation is not null
        || SslKeystorePassword is not null
        || EnableSslCertificateVerification is not null
        || SslEndpointIdentificationAlgorithm is not null;
}
