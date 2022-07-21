using ConductorSharp.Client.Model.Common;
using Newtonsoft.Json;

namespace ConductorSharp.Client.Model.Response
{
    public class GetTaskDefinitionResponse
    {
        [JsonProperty("result")]
        public TaskDefinition Result { get; set; }
    }
}
