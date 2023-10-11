using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Client.Model.Response
{
    public class PollDataResponse
    {
        [JsonProperty("queueName")]
        public string QueueName { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("workerId")]
        public string WorkerId { get; set; }

        [JsonProperty("lastPollTime")]
        public long LastPollTime { get; set; }
    }
}
