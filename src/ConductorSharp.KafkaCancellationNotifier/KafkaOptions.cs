using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ConductorSharp.KafkaCancellationNotifier
{
    internal class KafkaOptions
    {
        public string BootstrapServers { get; set; } = null!;
        public string TopicName { get; set; } = null!;
        public string GroupId { get; set; } = null!;
    }
}
