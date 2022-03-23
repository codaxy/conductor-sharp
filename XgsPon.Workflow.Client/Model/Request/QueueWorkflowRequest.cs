using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XgsPon.Workflows.Client.Model.Request
{
    public class QueueWorkflowRequest
    {
        [JsonProperty("input")]
        public JObject Input { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }
    }
}
