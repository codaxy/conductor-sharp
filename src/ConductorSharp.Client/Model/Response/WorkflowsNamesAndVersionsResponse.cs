using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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

    public class WorkflowNamesAndVersionsResponse
    {
        public Dictionary<string, List<NameAndVersion>> Data { get; set; }
    }
}
