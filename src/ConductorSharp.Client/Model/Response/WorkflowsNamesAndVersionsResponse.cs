using Newtonsoft.Json;

namespace ConductorSharp.Client.Model.Response
{
    public class NameAndVersion
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("createTime")]
        public long CreateTime { get; set; }
    }
}
