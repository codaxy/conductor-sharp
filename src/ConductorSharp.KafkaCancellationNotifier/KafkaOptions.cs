namespace ConductorSharp.KafkaCancellationNotifier;

internal class KafkaOptions
{
    public string BootstrapServers { get; set; } = null!;
    public string TopicName { get; set; } = null!;
    public string GroupId { get; set; } = null!;
    public bool CreateTopicOnStartup { get; set; }
}
