using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ConductorSharp.KafkaCancellationNotifier
{
    internal class KafkaOptions
    {
        [Required]
        public string BootstrapServers { get; set; }

        [Required]
        public string TopicName { get; set; }

        [Required]
        public string GroupId { get; set; }
    }
}
