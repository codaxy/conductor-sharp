using Newtonsoft.Json;
using XgsPon.Workflows.Client.Model.Common;

namespace XgsPon.Workflows.Client.Model.Response
{
    public class GetTaskDefinitionResponse
    {
        [JsonProperty("result")]
        public TaskDefinition Result { get; set; }
    }
}
